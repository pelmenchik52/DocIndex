using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KURSOVA.Model.Interfaces;

namespace KURSOVA.Model
{
    // Клас Indexer: Сервіс (частина Model), який створює та зберігає інвертований індекс корпусу.
    public class Indexer : IIndexer
    {
        // Сховища даних індексу 
        private Dictionary<string, double> _idfDictionary = new Dictionary<string, double>();
        // Зберігає TF-IDF вектор для кожного документа: <DocId, <Word, TF-IDF>>
        private Dictionary<int, Dictionary<string, double>> _documentVectors = new Dictionary<int, Dictionary<string, double>>();
        // Зберігає глобальну частоту слів (сума TF всіх документів)
        private Dictionary<string, int> _globalWordFrequency = new Dictionary<string, int>();
        // Зберігає повний набір унікальних слів корпусу
        private HashSet<string> _vocabulary = new HashSet<string>();

        //  Набір стоп-слів для української та англійської мов 
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            // Українські слова
            "і", "в", "з", "на", "та", "до", "не", "по", "для", "про", "він", "вона",
            "який", "яка", "яке", "які", "й", "цей", "ця", "ці", "того", "тому", "щоб",
            "а", "або", "але", "був", "була", "було", "були", "буде", "будуть", "бути",
            "від", "де", "як", "коли", "ми", "ви", "ти", "те", "це", "що", "один", "одна",
            "же", "чи", "за", "під", "над", "без", "між", "перед", "після",
            "їх", "його", "її", "їм", "нами", "вами", "них", "себе", "собі", "у", "із",
            "сказав", "сказала", "може", "можуть", "треба", "потрібно", "",
    
            // Англійські слова
            "a", "an", "the", "and", "but", "or", "as", "at", "by", "for", "from", "in", "into",
            "of", "off", "on", "out", "over", "to", "up", "with", "is", "was", "were", "be",
            "been", "being", "have", "has", "had", "do", "does", "did", "not", "no", "yes",
            "he", "she", "it", "they", "i", "you", "we", "me", "him", "her", "us", "them",
            "my", "your", "his", "her", "its", "our", "their", "this", "that", "these", "those",
            "can", "could", "would", "should", "will", "shall", "may", "might", "must",
            "about", "against", "among", "around", "before", "behind", "below", "between",
            "both", "each", "few", "more", "most", "other", "some", "such", "only", "too",
            "very", "just", "now", "then", "there", "here", "when", "where", "why", "how",
            "all", "any", "when", "while", "which", "who", "whom", "what", "than"
        };

        // Головний метод, який будує індекс з нуля для колекції документів.
        public void IndexCorpus(IEnumerable<IDocument> documents)
        {
            // 1. Очищення старого індексу
            ClearIndex();
            if (documents == null || !documents.Any()) return; // Захист від порожнього корпусу

            int totalDocCount = documents.Count();
            var docFrequency = new Dictionary<string, int>(); // n_t: Скільки документів містять слово t
            // Тимчасове сховище для WordCounts кожного документа
            var docTermFrequencies = new Dictionary<int, (Dictionary<string, int> wordCounts, int totalWords)>();

            // --- ЕТАП 1: Збір статистики (TF, n_t, Global Freq) ---
            foreach (var doc in documents)
            {
                // Токенізація, нормалізація, видалення стоп-слів
                var (wordCounts, totalWords) = Preprocess(doc.FullText);
                docTermFrequencies[doc.Id] = (wordCounts, totalWords);

                var uniqueWordsInDoc = new HashSet<string>();
                foreach (var (word, count) in wordCounts)
                {
                    // Оновлення глобальної частоти слова
                    _globalWordFrequency.TryAdd(word, 0);
                    _globalWordFrequency[word] += count;

                    uniqueWordsInDoc.Add(word);
                    _vocabulary.Add(word);
                }

                // Оновлення частоти документа (n_t)
                foreach (var uniqueWord in uniqueWordsInDoc)
                {
                    docFrequency.TryAdd(uniqueWord, 0);
                    docFrequency[uniqueWord]++;
                }
            }

            // --- ЕТАП 2: Обчислення IDF для кожного слова ---
            foreach (var word in _vocabulary)
            {
                int docsContaining = docFrequency[word];
                // Формула IDF: log10(N / (1 + n_t))
                _idfDictionary[word] = Math.Log10((double)totalDocCount / (1 + docsContaining));
            }

            // --- ЕТАП 3: Обчислення TF-IDF векторів для кожного документа ---
            foreach (var (docId, (wordCounts, totalWords)) in docTermFrequencies)
            {
                var docVector = new Dictionary<string, double>();
                if (totalWords == 0) continue; // Захист від ділення на нуль

                foreach (var (word, count) in wordCounts)
                {
                    double tf = (double)count / totalWords; // Термінова частота (Term Frequency)
                    double idf = _idfDictionary[word];      // Обернена частота документа
                    double tfIdf = tf * idf;                // Фінальна оцінка

                    if (tfIdf > 0) // Зберігаємо лише ненульові значення
                    {
                        docVector[word] = tfIdf;
                    }
                }
                _documentVectors[docId] = docVector;
            }
        }

        // Допоміжний метод: скидає всі індексні сховища.
        public void ClearIndex()
        {
            _idfDictionary.Clear();
            _documentVectors.Clear();
            _globalWordFrequency.Clear();
            _vocabulary.Clear();
        }

        // Допоміжний метод: токенізація, нормалізація та фільтрація тексту.
        public (Dictionary<string, int> wordCounts, int totalWords) Preprocess(string text)
        {
            var wordCounts = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(text))
            {
                return (wordCounts, 0); // Повертаємо порожній результат
            }
            // 1. toLower (нормалізація регістру)
            // 2. Regex.Split (токенізація за не-словами)
            // 3. Фільтрація: не пусті, довжина > 2, не є стоп-словом.
            var words = Regex.Split(text.ToLower(), @"\W+")
                             .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 2 && !StopWords.Contains(w));

            int totalWords = 0;
            foreach (var word in words)
            {
                wordCounts.TryAdd(word, 0);
                wordCounts[word]++;
                totalWords++;
            }
            // totalWords тут — це загальна кількість оброблених слів у цьому документі.
            return (wordCounts, totalWords);
        }

        // Методи доступу до даних (для Analyzer) 

        // Повертає TF-IDF вектор для конкретного документа.
        public Dictionary<string, double> GetDocumentVector(int docId)
        {
            _documentVectors.TryGetValue(docId, out var vector);
            // Повертає вектор або порожній словник, якщо документ не знайдено.
            return vector ?? new Dictionary<string, double>();
        }



        // Повертає словник IDF (word, idf_score)
        public Dictionary<string, double> GetIdfDictionary() => _idfDictionary;
        // Повертає глобальну частоту слів (word, total_count)
        public Dictionary<string, int> GetGlobalWordFrequencies() => _globalWordFrequency;
        // Повертає повний словник (усі унікальні слова) корпусу
        public HashSet<string> GetVocabulary() => _vocabulary;
        // Повертає всі TF-IDF вектори документів
        public Dictionary<int, Dictionary<string, double>> GetAllDocumentVectors() => _documentVectors;

    }
}