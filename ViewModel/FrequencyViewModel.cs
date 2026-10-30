using DocIndex.Commands;
using DocIndex.Model;
using DocIndex.Model.Interfaces;
using DocIndex.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using System.Windows.Input;

namespace DocIndex.ViewModel
{
    // ViewModel для відображення, фільтрації та експорту глобального словника частот.
    public class FrequencyViewModel : ViewModelBase
    {

        private Action _closeAction;
        private readonly IIndexer _indexer;
        private readonly IDialogService _dialogService; 

        // Колекція, яка містить об'єкти даних (Model).
        public ObservableCollection<FrequencyItem> FrequencyList { get; }
            = new ObservableCollection<FrequencyItem>();

        // ICollectionView: Шарування над колекцією для підтримки фільтрації та сортування.
        public ICollectionView FrequencyListView { get; }

        // Команди для взаємодії з View.
        public ICommand BtnExport { get; }
        public ICommand BtnOK { get; }

        private string _searchQuery;
        // Запит для фільтрації. Зміна викликає оновлення фільтра.
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                FrequencyListView.Refresh(); // Оновлюємо фільтр
            }
        }

        // Використовується DialogService для встановлення робочої функції Close()
        public void SetCloseAction(Action action)
        {
            _closeAction = action;
        }

        // Конструктор: отримує IIndexer для даних та Action для закриття вікна.
        public FrequencyViewModel(IIndexer indexer, Action closeAction,  IDialogService dialogService)
        {
            _indexer = indexer;
            _closeAction = closeAction;
            _dialogService = dialogService; // Збереження сервісу

            FrequencyListView = CollectionViewSource.GetDefaultView(FrequencyList);
            FrequencyListView.Filter = FilterFrequencyList;

            // Ініціалізація команд
            BtnExport = new RelayCommand(ExecuteExport, CanExecute);
            BtnOK = new RelayCommand(ExecuteCloseWindow, CanExecute);
        }

        // Метод, який завантажує, конвертує та сортує дані з Model (Indexer).
        public void LoadFrequencyDictionary()
        {
            FrequencyList.Clear();
            var globalFrequencies = _indexer.GetGlobalWordFrequencies();

            if (globalFrequencies == null || !globalFrequencies.Any())
            {
                return;
            }

            // Конвертуємо, сортуємо за спаданням частоти і додаємо до ObservableCollection.
            var sortedList = globalFrequencies
                .Select(kvp => new FrequencyItem { Word = kvp.Key, Count = kvp.Value })
                .OrderByDescending(item => item.Count);

            foreach (var item in sortedList)
            {
                FrequencyList.Add(item);
            }
        }

        // Логіка CanExecute для команд.
        private bool CanExecute(object obj)
        {
            return true;
        }

        // Логіка фільтрації: пошук слів, що починаються з введеного запиту.
        private bool FilterFrequencyList(object obj)
        {
            if (string.IsNullOrWhiteSpace(_searchQuery))
                return true;

            if (obj is FrequencyItem item)
            {
                return item.Word.StartsWith(_searchQuery, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // Дія експорту даних у файл.
        private void ExecuteExport(object obj)
        {
            // Використовуємо IDialogService для виклику діалогу збереження файлу.
            string filePath = _dialogService.ShowSaveFileDialog(
                filter: "CSV (Comma Separated Values)|*.csv|Text file (*.txt)|*.txt",
                defaultFileName: "frequency_dictionary.csv"
            );

            // Перевіряємо, чи користувач не скасував діалог.
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    // --- Логіка генерації CSV/тексту ---
                    // Використовуємо StringBuilder для ефективної побудови файлу.
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Word;Count"); // Заголовок CSV

                    // Ітеруємося по відфільтрованій колекції (ICollectionView)
                    foreach (FrequencyItem item in FrequencyListView)
                    {
                        // Записуємо дані у форматі CSV/текст.
                        sb.AppendLine($"{item.Word};{item.Count}");
                    }

                    // Зберігаємо у файл. Ця логіка залишається у ViewModel, оскільки це I/O операція.
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                    // Використовуємо IDialogService для повідомлення про успіх.
                    _dialogService.ShowMessage(
                        $"Dictionary is saved succesfuly to:\n{filePath}",
                        "Export"
                    );
                }
                catch (Exception ex)
                {
                    // Використовуємо IDialogService для повідомлення про помилку.
                    _dialogService.ShowMessage(
                        $"Error during saving file: {ex.Message}",
                        "Error"
                    );
                }
            }
        }


        // Дія закриття вікна через колбек.
        private void ExecuteCloseWindow(object obj)
        {
            // ViewModel викликає абстрактну дію, не знаючи про об'єкт Window.
            _closeAction?.Invoke();
        }
    }
}