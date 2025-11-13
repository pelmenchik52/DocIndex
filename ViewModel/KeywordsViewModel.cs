using KURSOVA.Commands;
using KURSOVA.Model.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System;
using KURSOVA.Model;
using KURSOVA.Properties.Localizations;
using KURSOVA.Services; 

namespace KURSOVA.ViewModel
{
    // ViewModel для відображення ключових слів, їхніх оцінок (TF-IDF) та статистики корпусу.
    public class KeywordsViewModel : ViewModelBase
    {
        private readonly IAnalyzer _analyzer;
        private readonly IDialogService _dialogService; 

        // Властивості даних 

        private string _corpusInfoText;
        // Загальний інформаційний рядок зі статистикою корпусу.
        public string CorpusInfoText
        {
            get => _corpusInfoText;
            set
            {
                _corpusInfoText = value;
                OnPropertyChanged(nameof(CorpusInfoText));
            }
        }

        // Колекція ключових слів, прив'язана до View.
        public ObservableCollection<KeywordItem> Keywords { get; }

        // Команди 
        public ICommand ExportKeywordsCommand { get; }

        //  Конструктор
        public KeywordsViewModel(IAnalyzer analyzer, IDialogService dialogService)
        {
            _analyzer = analyzer;      
            _dialogService = dialogService; 

            Keywords = new ObservableCollection<KeywordItem>();

            ExportKeywordsCommand = new RelayCommand(ExportKeywordsAction, CanExport);
        }

        // Логіка завантаження даних 

        // Метод, який викликається ManagementVM після успішної індексації.
        public void LoadKeywordsAction()
        {
            Keywords.Clear();

            // 1. Отримуємо ключові слова з Analyzer.
            var keywordsList = _analyzer.GetCorpusKeywords().ToList();

            // 2. Додаємо до ObservableCollection (обмежено 100 найбільш важливими).
            foreach (var item in keywordsList.Take(100))
            {
                Keywords.Add(item);
            }

            // 3. Оновлюємо стан команди (CanExecute).
            (ExportKeywordsCommand as RelayCommand)?.RaiseCanExecuteChanged();

            // 4. Формування інформаційного рядка.
            if (Keywords.Any())
            {
                string mostImportantWord = Keywords.First().Word;
                int uniqueWords = _analyzer.GetUniqueWordCount();
                int totalWords = _analyzer.GetTotalProcessedWordCount();

                string formatString = Resources.Keywords_Info_Stats;

                // Використовуємо string.Format для підстановки даних.
                CorpusInfoText = string.Format(
                    formatString,
                    mostImportantWord,
                    uniqueWords,
                    totalWords
                );
            }
            else
            {
                CorpusInfoText = Resources.Keywords_Info_Empty;
            }
        }

        // Логіка CanExecute
        private bool CanExport(object obj)
        {
            // Дозволяємо експорт, лише якщо колекція не порожня.
            return Keywords.Any();
        }

        // Логіка експорту
        private void ExportKeywordsAction(object obj)
        {
            // 1. Використовуємо IDialogService для отримання шляху збереження.
            string filePath = _dialogService.ShowSaveFileDialog(
                filter: "CSV Files (*.csv)|*.csv",
                defaultFileName: $"Keywords_{DateTime.Now:yyyyMMdd}.csv"
            );

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    // 2. Генерація контенту CSV (I/O логіка).
                    var sb = new StringBuilder();
                    sb.AppendLine("Word;Score;Frequency");

                    foreach (var k in Keywords)
                    {
                        // Форматування Score до 4 знаків після коми.
                        sb.AppendLine($"{k.Word};{k.Score:F4};{k.Frequency}");
                    }

                    // 3. Запис у файл.
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                    // 4. Повідомлення про успіх через IDialogService.
                    _dialogService.ShowMessage(
                        $"Keywords succesfuly saved to:\n{filePath}",
                        "Succes"
                    );
                }
                catch (Exception ex)
                {
                    // 5. Повідомлення про помилку через IDialogService.
                    _dialogService.ShowMessage(
                        $"Error during saving file: {ex.Message}",
                        "Error"
                    );
                }
            }
        }
    }
}