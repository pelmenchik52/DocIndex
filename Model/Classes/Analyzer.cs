using KURSOVA.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    // Клас Analyzer: виконує складну бізнес-логіку (TF-IDF аналіз, пошук подібності).
    // Він є частиною Model і використовує індекс для обчислень.
    public class Analyzer : IAnalyzer
    {
        private readonly IIndexer _indexer;
        private readonly Corpus _corpus;

        // Dependency Injection: Analyzer отримує готовий індексер через конструктор.
        public Analyzer(IIndexer indexer)
        {
            _indexer = indexer;
            // Отримуємо Singleton-екземпляр корпусу документів.
            _corpus = Corpus.Instance;
        }

        // Метод обчислює оцінку "ключовості" кожного слова у всьому корпусі.
        public IEnumerable<KeywordItem> GetCorpusKeywords()
        {
            // Отримуємо глобальні частоти слів та IDF словник з індексера.
            var globalWordFrequency = _indexer.GetGlobalWordFrequencies();
            var idfDictionary = _indexer.GetIdfDictionary();

            // Перевірка наявності даних
            if (globalWordFrequency == null || !idfDictionary.Any())
            {
                return Enumerable.Empty<KeywordItem>();
            }

            // Тимчасовий список для зберігання результатів.
            var keywordList = new List<KeywordItem>();

            // 1. Розрахунок TF * IDF
            foreach (var (word, totalFreq) in globalWordFrequency)
            {
                // Пропускаємо слово, якщо воно не має розрахованого IDF (хоча це малоймовірно, якщо індекс коректний).
                if (!idfDictionary.ContainsKey(word)) continue;

                // Отримуємо IDF для слова.
                double idf = idfDictionary[word];
                // Обчислюємо Score: це міра важливості слова у всьому корпусі.
                double score = totalFreq * idf;

                keywordList.Add(new KeywordItem { Word = word, Frequency = totalFreq, Score = score });
            }

            // 2. Сортуємо слова за спаданням їхньої оцінки ключовості.
            return keywordList.OrderByDescending(k => k.Score);
        }

        // Повертає кількість унікальних термінів у всьому корпусі.
        public int GetUniqueWordCount()
        {
            // Використовуємо оператор умовного null '??' для безпечного повернення 0, якщо індексер ще не готовий.
            return _indexer.GetVocabulary()?.Count ?? 0;
        }

        // Повертає загальну кількість всіх слів, оброблених у корпусі (включаючи повтори).
        public int GetTotalProcessedWordCount()
        {
            // Сумує значення (частоти) у словнику глобальних частот.
            return _indexer.GetGlobalWordFrequencies()?.Values.Sum() ?? 0;
        }

        // Обчислює косинусну подібність між цільовим документом і всіма іншими в корпусі.
        public List<SimilarityResult> FindSimilarDocuments(IDocument targetDocument, double similarityThreshold = 0.1)
        {
            var results = new List<SimilarityResult>();
            if (targetDocument == null) return results;

            var targetVector = _indexer.GetDocumentVector(targetDocument.Id);
            if (!targetVector.Any()) return results;

            var allVectors = _indexer.GetAllDocumentVectors();

            // Обчислюємо норму цільового вектора (|A|) ОДИН РАЗ
            double normA = CalculateNorm(targetVector);

            foreach (var (docId, vectorB) in allVectors)
            {
                if (docId == targetDocument.Id) continue;

                double similarity = CalculateCosineSimilarity(targetVector, normA, vectorB);

                if (similarity >= similarityThreshold)
                {
                    var doc = _corpus.GetDocumentById(docId);
                    results.Add(new SimilarityResult
                    {
                        DocumentId = docId,
                        DocumentTitle = doc?.Title ?? $"Document {docId}",
                        Score = similarity
                    });
                }
            }

            return results.OrderByDescending(r => r.Score).ToList();
        }
        public List<SimilarityResult> SearchByQuery(string query, double similarityThreshold)
        {
            // Попередня обробка запиту (токенізація, нормалізація, видалення стоп-слів).
            var queryPrep = _indexer.Preprocess(query);
            var queryWordCounts = queryPrep.wordCounts;
            var totalQueryWords = queryPrep.totalWords;

            // Якщо запит порожній після обробки, повертаємо порожній результат.
            if (totalQueryWords == 0)
                return new List<SimilarityResult>();

            // 1. Створюємо Query Vector (вектор запиту)
            var idfDictionary = _indexer.GetIdfDictionary();
            // Перевірка, чи побудовано індекс (наявність IDF словника).
            if (idfDictionary == null || !idfDictionary.Any())
                return new List<SimilarityResult>();

            var queryVector = new Dictionary<string, double>();
            double normQuery = 0.0; // Для обчислення норми вектора запиту.

            // Обчислення TF-IDF ваг для кожного слова в запиті.
            foreach (var (word, count) in queryWordCounts)
            {
                if (idfDictionary.ContainsKey(word))
                {
                    // Обчислення TF (Term Frequency)
                    double tf = (double)count / totalQueryWords;
                    // Отримання IDF (Inverse Document Frequency)
                    double idf = idfDictionary[word];
                    // Обчислення TF-IDF ваги
                    double tfIdf = tf * idf;

                    queryVector[word] = tfIdf;
                    // Накопичення суми квадратів для обчислення норми.
                    normQuery += Math.Pow(tfIdf, 2);
                }
            }
            // Фінальне обчислення норми вектора запиту (|A|).
            normQuery = Math.Sqrt(normQuery);

            // Запобігання діленню на нуль, якщо TF-IDF ваги виявилися нульовими.
            if (normQuery == 0)
                return new List<SimilarityResult>();

            // 2. Порівнюємо вектор запиту з усіма векторами документів (Query vs Corpus)
            var allVectors = _indexer.GetAllDocumentVectors();
            var results = new List<SimilarityResult>();

            // Ітеруємося по всіх векторах документів у корпусі.
            foreach (var (docId, vectorB) in allVectors)
            {
                // Обчислюємо косинусну подібність між вектором запиту та вектором документа.
                double similarity = CalculateCosineSimilarity(queryVector, normQuery, vectorB);

                // 5. Додаємо в результати, якщо подібність перевищує поріг.
                if (similarity >= similarityThreshold)
                {
                    var doc = _corpus.GetDocumentById(docId);
                    results.Add(new SimilarityResult
                    {
                        DocumentId = docId,
                        DocumentTitle = doc?.Title ?? $"Document {docId}",
                        Score = similarity // Оцінка подібності
                    });
                }
            }

            // 6. Сортуємо результати за спаданням оцінки подібності (ранжування).
            return results.OrderByDescending(r => r.Score).ToList();
        }

        // Допоміжний метод: Обчислює евклідову норму (довжину) вектора (|A|).
        private double CalculateNorm(Dictionary<string, double> vector)
        {
            double norm = 0.0;
            // Сумуємо квадрати всіх компонентів.
            foreach (var (word, tfIdf) in vector)
            {
                norm += Math.Pow(tfIdf, 2);
            }
            // Повертаємо квадратний корінь.
            return Math.Sqrt(norm);
        }

        // Допоміжний метод: Обчислює косинусну подібність між двома векторами TF-IDF.
        // Cos(θ) = (A · B) / (|A| * |B|)
        private double CalculateCosineSimilarity(Dictionary<string, double> vecA, double normA, Dictionary<string, double> vecB)
        {
            double dotProduct = 0.0; // Скалярний добуток (A · B)
            double normB = 0.0;      // Норма вектора B (|B|)
            double similarity = 0.0;

            // Оптимізація: ітеруємося по меншому вектору для ефективного обчислення скалярного добутку.
            var smallerVec = vecA.Count < vecB.Count ? vecA : vecB;
            var largerVec = vecA.Count < vecB.Count ? vecB : vecA;

            // Рахуємо скалярний добуток (A · B): сума добутків спільних термінів.
            foreach (var (word, tfIdf) in smallerVec)
            {
                if (largerVec.TryGetValue(word, out double otherTfIdf))
                {
                    dotProduct += tfIdf * otherTfIdf;
                }
            }

            // Рахуємо норму вектора B (|B|) за допомогою допоміжного методу.
            normB = CalculateNorm(vecB);

            // Запобігання діленню на нуль.
            if (normA != 0 && normB != 0)
            {
                // Фінальна формула косинусної подібності.
                similarity = dotProduct / (normA * normB);
            }

            return similarity;
        }
    }
}