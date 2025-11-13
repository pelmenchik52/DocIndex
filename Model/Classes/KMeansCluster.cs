using KURSOVA.Model.Classes;
using KURSOVA.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KURSOVA.Model.Interfaces;
using System.Threading; 

namespace KURSOVA.Model.Classes
{
    // Клас для виконання кластеризації K-Means на векторах TF-IDF.
    public class KMeansCluster : IKMeansCluster
    {
        private readonly IIndexer _indexer;
        // Зберігаємо всі вектори документів під час виконання.
        private Dictionary<int, Dictionary<string, double>> _documentVectors;
        private readonly Random _random = new Random();

        // Oтримуємо доступ до даних індексу.
        public KMeansCluster(IIndexer indexer)
        {
            _indexer = indexer;
        }

        // Головний публічний метод: запускає алгоритм кілька разів (k-means++) для пошуку найкращого результату.
        public List<Cluster> Cluster(int k, int maxIterations = 100)
        {
            // Кількість запусків для запобігання поганій випадковій ініціалізації.
            int numRuns = 10;

            List<Cluster> bestClusters = new List<Cluster>();
            double lowestSSE = double.MaxValue; // SSE: Sum of Squared Errors (сума квадратів помилок)

            // Запускаємо K-Means N разів.
            for (int run = 0; run < numRuns; run++)
            {
                // Запускаємо повний алгоритм з унікальною випадковою ініціалізацією.
                List<Cluster> currentClusters = RunSingleIteration(k, maxIterations);

                // Оцінюємо якість поточної кластеризації.
                double currentSSE = CalculateSSE(currentClusters);

                // Зберігаємо найкращий результат (найменша SSE означає щільніші кластери).
                if (currentSSE < lowestSSE)
                {
                    lowestSSE = currentSSE;
                    bestClusters = currentClusters;
                }
            }

            return bestClusters;
        }

        // Приватний метод: виконує один повний запуск алгоритму K-Means.
        private List<Cluster> RunSingleIteration(int k, int maxIterations)
        {
            // Отримуємо TF-IDF вектори з індексера.
            _documentVectors = _indexer.GetAllDocumentVectors();

            if (_documentVectors.Count < k)
            {
                // Якщо документів менше, ніж кластерів, кластеризація неможлива.
                return new List<Cluster>();
            }

            // 1. Ініціалізація центроїдів (випадковим чином обираємо k документів).
            List<Dictionary<string, double>> centroids = InitializeCentroids(k);

            Dictionary<int, int> assignments = new Dictionary<int, int>(); // docId -> clusterIndex
            bool changed = true;
            int iterations = 0;

            // 2. Головний цикл k-means: триває, доки призначення змінюються або досягнуто maxIterations.
            while (changed && iterations < maxIterations)
            {
                changed = false;
                iterations++;

                // Етап Призначення (Assignment): Кожен документ призначається до найближчого центроїда.
                var newAssignments = new Dictionary<int, int>();
                foreach (var (docId, docVector) in _documentVectors)
                {
                    // Знаходимо центроїд з максимальною косинусною подібністю (мінімальною "відстанню").
                    int nearestClusterIndex = FindNearestCentroid(docVector, centroids);
                    newAssignments[docId] = nearestClusterIndex;

                    // Перевіряємо, чи змінилося призначення порівняно з попередньою ітерацією.
                    if (!assignments.ContainsKey(docId) || assignments[docId] != nearestClusterIndex)
                    {
                        changed = true;
                    }
                }
                assignments = newAssignments;

                // Етап Оновлення (Update): Перераховуємо нові центроїди (центр мас).
                centroids = UpdateCentroids(k, assignments);
            }

            // 3. Форматування результатів у фінальний список Cluster.
            return FormatResults(k, centroids, assignments);
        }

        // Допоміжний метод для обчислення Суми Квадратів Помилок (SSE).
        // SSE використовується для порівняння якості різних запусків алгоритму.
        private double CalculateSSE(List<Cluster> clusters)
        {
            double totalSSE = 0;
            foreach (var cluster in clusters)
            {
                var centroid = cluster.Centroid;
                foreach (var docId in cluster.DocumentIds)
                {
                    var docVector = _indexer.GetDocumentVector(docId);

                    // Використовуємо косинусну подібність.
                    double similarity = CalculateCosineSimilarity(docVector, centroid);

                    // У просторі косинусної подібності "Відстань" = 1 - "Схожість".
                    double distance = 1.0 - similarity;

                    // Додаємо квадрат відстані.
                    totalSSE += Math.Pow(distance, 2);
                }
            }
            return totalSSE;
        }

        // Ініціалізує центроїди, випадково вибираючи k документів.
        private List<Dictionary<string, double>> InitializeCentroids(int k)
        {
            var docIds = _documentVectors.Keys.ToList();
            var initialCentroids = new List<Dictionary<string, double>>();

            // Випадково перемішуємо ID документів і беремо перші k.
            var shuffledIds = docIds.OrderBy(id => _random.Next()).Take(k).ToList();

            foreach (var docId in shuffledIds)
            {
                // Центроїд ініціалізується копією вектора документа.
                initialCentroids.Add(new Dictionary<string, double>(_documentVectors[docId]));
            }
            return initialCentroids;
        }

        // Знаходить центроїд, найближчий до вектора документа (найвища косинусна подібність).
        private int FindNearestCentroid(Dictionary<string, double> docVector, List<Dictionary<string, double>> centroids)
        {
            int bestClusterIndex = 0;
            double maxSimilarity = -1.0;

            for (int i = 0; i < centroids.Count; i++)
            {
                double similarity = CalculateCosineSimilarity(docVector, centroids[i]);
                if (similarity > maxSimilarity)
                {
                    maxSimilarity = similarity;
                    bestClusterIndex = i;
                }
            }
            return bestClusterIndex;
        }

        // Обчислює нове розташування центроїдів як середнє арифметичне векторів у кластері.
        private List<Dictionary<string, double>> UpdateCentroids(int k, Dictionary<int, int> assignments)
        {
            var newCentroids = new List<Dictionary<string, double>>();
            var clusterCounts = new int[k];

            for (int i = 0; i < k; i++)
            {
                newCentroids.Add(new Dictionary<string, double>());
            }

            // 1. Сумуємо всі вектори в кожному кластері.
            foreach (var (docId, clusterIndex) in assignments)
            {
                clusterCounts[clusterIndex]++;
                var docVector = _documentVectors[docId];

                foreach (var (word, tfIdf) in docVector)
                {
                    // Накопичуємо суму компонентів вектора
                    newCentroids[clusterIndex].TryAdd(word, 0.0);
                    newCentroids[clusterIndex][word] += tfIdf;
                }
            }

            // 2. Ділимо суму на кількість документів, щоб отримати середнє (центр мас).
            for (int i = 0; i < k; i++)
            {
                int docCountInCluster = clusterCounts[i];
                if (docCountInCluster > 0)
                {
                    var centroid = newCentroids[i];
                    var dimensions = centroid.Keys.ToList();

                    foreach (var word in dimensions)
                    {
                        centroid[word] /= docCountInCluster; // Ділення на кількість для середнього
                    }
                }
                else
                {
                    // Обробка порожнього кластера: переініціалізуємо його випадковим документом.
                    var docIds = _documentVectors.Keys.ToList();
                    int randomDocId = docIds[_random.Next(docIds.Count)];
                    newCentroids[i] = new Dictionary<string, double>(_documentVectors[randomDocId]);
                }
            }
            return newCentroids;
        }

        // Форматує тимчасові результати (центроїди та призначення) у фінальний список Cluster.
        private List<Cluster> FormatResults(int k, List<Dictionary<string, double>> centroids, Dictionary<int, int> assignments)
        {
            var clusters = new List<Cluster>();
            for (int i = 0; i < k; i++)
            {
                // Створюємо об'єкт Cluster з його фінальним центроїдом.
                clusters.Add(new Cluster(i, centroids[i]));
            }

            // Додаємо ID документів у відповідні кластери.
            foreach (var (docId, clusterIndex) in assignments)
            {
                clusters[clusterIndex].DocumentIds.Add(docId);
            }

            return clusters;
        }

        // Обчислює косинусну подібність між двома TF-IDF векторами.
        private double CalculateCosineSimilarity(Dictionary<string, double> vecA, Dictionary<string, double> vecB)
        {
            if (vecA == null || vecB == null || !vecA.Any() || !vecB.Any())
                return 0.0;

            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            // Обчислення скалярного добутку (A · B) та норми A (|A|)
            foreach (var (word, tfIdfA) in vecA)
            {
                normA += Math.Pow(tfIdfA, 2);
                if (vecB.TryGetValue(word, out double tfIdfB))
                {
                    dotProduct += tfIdfA * tfIdfB;
                }
            }
            normA = Math.Sqrt(normA);

            // Обчислення норми B (|B|)
            foreach (var (word, tfIdfB) in vecB)
            {
                normB += Math.Pow(tfIdfB, 2);
            }
            normB = Math.Sqrt(normB);

            if (normA == 0 || normB == 0)
                return 0.0;

            // Cos(θ) = (A · B) / (|A| * |B|)
            return dotProduct / (normA * normB);
        }
    }
}