using KURSOVA.Commands;
using KURSOVA.Model;
using KURSOVA.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KURSOVA.ViewModel
{
    public class KeywordsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<KeywordItem> Keywords { get; set; }
        public ICommand ExportKeywordsCommand { get; set; }


        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "і", "в", "з", "на", "та", "до", "не", "по", "для", "про", "він", "вона",
            "це", "що", "як", "а", "але", "був", "була", "було", "були", "його", "її",
            "ми", "ви", "вони", "від", "за", "під", "над", "при", "про", "без", "ти",
            "ж", "б", "чи", "тільки", "ще", "вже", "або", "я", "у", "зі", "проте",
            "який", "яка", "яке", "які", "й", "цей", "ця", "ці", "того", "тому", "щоб", "1"
        };

        public KeywordsViewModel()
        {
            Keywords = new ObservableCollection<KeywordItem>();
            ExportKeywordsCommand = new RelayCommand(ExportKeywordsAction, CanExport);

            Corpus.Instance.Documents.CollectionChanged += (s, e) => CalculateKeywords();

            // Первинний розрахунок при запуску
            CalculateKeywords();
        }
        private bool CanExport(object obj)
        {
            // Дозволяємо експорт, лише якщо є що експортувати
            return Keywords.Count > 0;
        }
        private void CalculateKeywords()
        {
            var allDocs = Corpus.Instance.Documents;
            if (allDocs.Count == 0)
            {
                Keywords.Clear();
                return ;
            }

            int totalDocCount = allDocs.Count;

            var globalWordFrequency = new Dictionary<string, int>();

            var docFrequency = new Dictionary<string, int>();
            foreach (var doc in allDocs)
            {
                // Токенізація: ділимо текст на слова, прибираємо знаки пунктуації,
                // переводимо в нижній регістр та ігноруємо стоп-слова.
                var words = Regex.Split(doc.FullText.ToLower(), @"\W+")
                                 .Where(w => !string.IsNullOrWhiteSpace(w) &&
                                             w.Length > 2 && // Ігноруємо слова з 2-х і менше літер
                                             !StopWords.Contains(w));

                var uniqueWordsInDoc = new HashSet<string>();

                foreach (var word in words)
                {
                    // Додаємо до загальної частоти
                    globalWordFrequency.TryAdd(word, 0);
                    globalWordFrequency[word]++;

                    // Додаємо до списку унікальних слів для цього документа
                    uniqueWordsInDoc.Add(word);
                }

                // Оновлюємо лічильник "Document Frequency"
                foreach (var uniqueWord in uniqueWordsInDoc)
                {
                    docFrequency.TryAdd(uniqueWord, 0);
                    docFrequency[uniqueWord]++;
                }
            }

            // --- КРОК 2: Розраховуємо TF-IDF та заповнюємо колекцію ---
            var keywordList = new List<KeywordItem>();
            foreach (var (word, totalFreq) in globalWordFrequency)
            {
                if (!docFrequency.ContainsKey(word)) continue;

                int docsContaining = docFrequency[word];

                // Розрахунок IDF
                // (Беремо log10, щоб уникнути занадто великих чисел)
                double idf = Math.Log10((double)totalDocCount / docsContaining);

                // Наш "Score" - це загальна частота, помножена на "важливість" слова
                double score = totalFreq * idf;

                keywordList.Add(new KeywordItem
                {
                    Word = word,
                    Frequency = totalFreq,
                    Score = score
                });
            }

            // --- КРОК 3: Оновлюємо UI ---
            // Сортуємо за балами (Score) і беремо, наприклад, 100 найкращих
            var sortedKeywords = keywordList.OrderByDescending(k => k.Score).Take(100);

            Keywords.Clear();
            foreach (var k in sortedKeywords)
            {
                Keywords.Add(k);
            }

            // Оновлюємо стан кнопки "Експорт"
            (ExportKeywordsCommand as RelayCommand)?.RaiseCanExecuteChanged();

        
        }
        private void ExportKeywordsAction(object obj)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Keywords_{DateTime.Now:yyyyMMdd}.csv",
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Word;Score;Frequency"); // Заголовки

                    foreach (var k in Keywords)
                    {
                        sb.AppendLine($"{k.Word};{k.Score};{k.Frequency}"); // Дані
                    }

                    File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Експорт успішно завершено!", "Експорт",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні файлу: {ex.Message}", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
