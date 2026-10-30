using DocIndex.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocIndex.Model.Interfaces;

namespace DocIndex.ViewModel
{
    // ViewModel для відображення статистичних метрик корпусу (довжина, частота, лексичне багатство, рекорди тощо).
    public class StatsViewModel : ViewModelBase
    {
        private readonly IIndexer _indexer;

        // 1. Загальна статистика 
        
        private int _documentCount;
        // Загальна кількість документів у корпусі.
        public int DocumentCount
        {
            get => _documentCount;
            set { _documentCount = value; OnPropertyChanged(nameof(DocumentCount)); }
        }

        private long _totalWordCount;
        // Загальна кількість оброблених слів (з повторами).
        public long TotalWordCount
        {
            get => _totalWordCount;
            set { _totalWordCount = value; OnPropertyChanged(nameof(TotalWordCount)); }
        }

        private int _uniqueWordCount;
        // Кількість унікальних слів (розмір словника).
        public int UniqueWordCount
        {
            get => _uniqueWordCount;
            set { _uniqueWordCount = value; OnPropertyChanged(nameof(UniqueWordCount)); }
        }

        // 2. Просунута статистика
        
        private double _lexicalRichness; // TTR (Type-Token Ratio)
        // Лексичне багатство: UniqueWordCount / TotalWordCount.
        public double LexicalRichness
        {
            get => _lexicalRichness;
            set { _lexicalRichness = value; OnPropertyChanged(nameof(LexicalRichness)); }
        }

        private int _hapaxLegomena; // Слова, що зустрілись 1 раз.
        // Кількість слів, які мають частоту 1.
        public int HapaxLegomena
        {
            get => _hapaxLegomena;
            set { _hapaxLegomena = value; OnPropertyChanged(nameof(HapaxLegomena)); }
        }

        // 3. Статистика документів 
        
        private string _longestDocument = "-";
        // Назва та довжина найдовшого документа.
        public string LongestDocument
        {
            get => _longestDocument;
            set { _longestDocument = value; OnPropertyChanged(nameof(LongestDocument)); }
        }

        private string _shortestDocument = "-";
        // Назва та довжина найкоротшого документа.
        public string ShortestDocument
        {
            get => _shortestDocument;
            set { _shortestDocument = value; OnPropertyChanged(nameof(ShortestDocument)); }
        }

        private double _avgDocLength;
        // Середня довжина документа у словах.
        public double AvgDocLength
        {
            get => _avgDocLength;
            set { _avgDocLength = value; OnPropertyChanged(nameof(AvgDocLength)); }
        }

        // 4. Статистика слів 
        
        private double _avgWordLength;
        // Середня довжина слова у символах.
        public double AvgWordLength
        {
            get => _avgWordLength;
            set { _avgWordLength = value; OnPropertyChanged(nameof(AvgWordLength)); }
        }

        private string _longestWord = "-";
        // Найдовше слово у словнику.
        public string LongestWord
        {
            get => _longestWord;
            set { _longestWord = value; OnPropertyChanged(nameof(LongestWord)); }
        }
        
        // Конструктор
        public StatsViewModel(IIndexer indexer)
        {
            // Отримуємо індексатор для доступу до даних.
            _indexer = indexer;
        }

        // Метод: Завантажує та обчислює всі статистичні метрики.
        public void LoadStats()
        {
            // Отримуємо "сирі" дані з Model (Corpus та Indexer) один раз.
            var documents = Corpus.Instance.Documents.ToList();
            var globalFrequencies = _indexer.GetGlobalWordFrequencies();
            var vocabulary = _indexer.GetVocabulary();

            // Скидання (Обробка порожнього індексу)
            if (!documents.Any() || globalFrequencies == null || !globalFrequencies.Any() || vocabulary == null)
            {
                // Скидаємо всі властивості, якщо дані відсутні.
                DocumentCount = 0;
                TotalWordCount = 0;
                UniqueWordCount = 0;
                LexicalRichness = 0;
                HapaxLegomena = 0;
                LongestDocument = "-";
                ShortestDocument = "-";
                AvgDocLength = 0;
                AvgWordLength = 0;
                LongestWord = "-";

                return;
            }

            // 1. Загальна статистика 
            DocumentCount = documents.Count;
            TotalWordCount = globalFrequencies.Values.Sum();
            UniqueWordCount = vocabulary.Count;

            // 2. Просунута статистика 
            LexicalRichness = (TotalWordCount > 0) ? (double)UniqueWordCount / TotalWordCount : 0;
            HapaxLegomena = globalFrequencies.Values.Count(freq => freq == 1);

            // 3. Статистика документів
            AvgDocLength = documents.Average(d => d.WordCount);

            var sortedByLength = documents.OrderByDescending(d => d.WordCount).ToList();
            var longest = sortedByLength.FirstOrDefault();
            var shortest = sortedByLength.LastOrDefault();

            LongestDocument = (longest != null) ? $"{longest.Title} ({longest.WordCount} слів)" : "-";
            ShortestDocument = (shortest != null) ? $"{shortest.Title} ({shortest.WordCount} слів)" : "-";

            // 4. Статистика слів 
            AvgWordLength = vocabulary.Average(w => w.Length);
            LongestWord = vocabulary.OrderByDescending(w => w.Length).FirstOrDefault() ?? "-";

        }
    }
}