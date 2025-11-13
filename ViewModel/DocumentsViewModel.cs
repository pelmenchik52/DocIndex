using KURSOVA.Commands;
using KURSOVA.Model;
using KURSOVA.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using KURSOVA.Services;

namespace KURSOVA.ViewModel
{
    // ViewModel для керування списком документів (View: DocumentsView).
    public class DocumentsViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainVM;
        private readonly IAnalyzer _analyzer;
        private readonly IDialogService _dialogService; // Впроваджений сервіс для чистих взаємодій з UI.

        // ICollectionView: Представлення колекції, що підтримує фільтрацію, прив'язане до Corpus.
        public ICollectionView Documents { get; }

        // Властивості стану 

        private bool _isDocumentSelected;
        // Чи вибраний документ; керує CanExecute команд та видимістю елементів UI.
        public bool IsDocumentSelected
        {
            get => _isDocumentSelected;
            private set
            {
                _isDocumentSelected = value;
                OnPropertyChanged(nameof(IsDocumentSelected));
            }
        }

        private string _searchQuery;
        // Запит для фільтрації списку документів.
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                // Негайно оновлюємо фільтр при зміні запиту.
                Documents.Refresh();
            }
        }

        // --- Команди ---
        public ICommand btnDeleteDocument { get; }
        public ICommand btnEditDocument { get; }
        public ICommand DocumentDropCommand { get; }
        public ICommand SearchRankedCommand { get; }

        //  Конструктор
        public DocumentsViewModel(MainViewModel mainVM, IDialogService dialogService, IAnalyzer analyzer)
        {
            _mainVM = mainVM;
            _dialogService = dialogService; // Зберігаємо впроваджений сервіс.

            // Ініціалізація ICollectionView.
            Documents = CollectionViewSource.GetDefaultView(Corpus.Instance.Documents);
            Documents.Filter = FilterDocuments;

            // Ініціалізація команд.
            btnDeleteDocument = new RelayCommand(DeleteDocumentAction, CanExecuteEditOrDelete);
            btnEditDocument = new RelayCommand(EditDocumentAction, CanExecuteEditOrDelete);
            DocumentDropCommand = new RelayCommand(async (o) => await OnDocumentDropAsync(o), o => true);
            SearchRankedCommand = new RelayCommand(async (o) => await ExecuteRankedSearchAsync(), CanExecuteRankedSearch);

            // Підписка на зміни у батьківській ViewModel для оновлення CanExecute.
            _mainVM.PropertyChanged += OnMainVmPropertyChanged;
            _analyzer = analyzer;
        }

        // Перевірка, чи побудований індекс (для ранжованого пошуку).
        private bool IsIndexReady
        {
            // Читаємо стан IsIndexBuilt з ManagementVM, який зберігається у MainVM
            get => _mainVM.ManagementVM?.IsIndexBuilt ?? false;
        }

        // Логіка фільтрації 
        private bool FilterDocuments(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return true;

            if (obj is IDocument doc)
            {
                // Фільтрація: пошук у заголовку або повному тексті.
                return doc.Title.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                       doc.FullText.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        // Обробник змін SelectedDocument 
        private void OnMainVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedDocument))
            {
                IsDocumentSelected = (_mainVM.SelectedDocument != null);
                // Повідомляємо View, що стан команд змінився.
                (btnDeleteDocument as RelayCommand)?.RaiseCanExecuteChanged();
                (btnEditDocument as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Логіка CanExecute
        private bool CanExecuteEditOrDelete(object obj)
        {
            return _mainVM.SelectedDocument != null;
        }

        // Логіка виконання команд
        // Видалення документа з підтвердженням.
        private void DeleteDocumentAction(object obj)
        {
            if (_mainVM.SelectedDocument != null)
            {
                // Використовуємо IDialogService для підтвердження.
                bool confirmed = _dialogService.ShowConfirmation(
                    $"Ви впевнені, що хочете видалити документ?\n{_mainVM.SelectedDocument.Title}",
                    "Підтвердження видалення");

                if (confirmed)
                {
                    // Видаляємо з Model. ObservableCollection оновлює View.
                    Corpus.Instance.Documents.Remove(_mainVM.SelectedDocument);
                    _mainVM.SelectedDocument = null;
                }
            }
        }
        // Редагування вибраного документа.
        private void EditDocumentAction(object obj)
        {
            var docToEdit = _mainVM.SelectedDocument;

            if (docToEdit == null) return;

            // Використовуємо IDialogService для відкриття модального вікна (Чистий MVVM).
            _dialogService.ShowEditDocumentDialog(docToEdit);
        }

        // Асинхронний метод для обробки перетягування файлів.
        private async Task OnDocumentDropAsync(object obj)
        {
            if (obj is not DragEventArgs e) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var textFiles = files.Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).ToList();
                if (!textFiles.Any()) return;

                _mainVM.IsBusy = true;
                _mainVM.CurrentStatusMessage = $"Processing {textFiles.Count} dropped files...";

                int importedCount = 0;

                try
                {
                    // Асинхронна фонова операція: створюємо об'єкти Document (завантаження I/O).
                    var docsToAdd = await Task.Run(() =>
                    {
                        var docs = new List<IDocument>();
                        foreach (string filename in textFiles)
                        {
                            try { docs.Add(new Document(filename)); } catch { }
                        }
                        return docs;
                    });

                    // Повертаємося до UI-потоку для оновлення ObservableCollection.
                    foreach (var doc in docsToAdd)
                    {
                        Corpus.Instance.AddDocument(doc);
                        importedCount++;
                    }
                    _mainVM.CurrentStatusMessage = $"Import completed. Successfully added {importedCount} documents. Ready.";
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Помилка при перетягуванні файлів: {ex.Message}", "Помилка");
                    _mainVM.CurrentStatusMessage = $"Error: {ex.Message}";
                }
                finally
                {
                    _mainVM.IsBusy = false;
                }
            }
        }
        private bool CanExecuteRankedSearch(object obj)
        {
            // Дозволяємо ранжований пошук, якщо поле не порожнє.
            return !string.IsNullOrWhiteSpace(SearchQuery);
        }
        private async Task ExecuteRankedSearchAsync()
        {
            string query = SearchQuery;

            // Перевірка: Чи побудовано індекс?
            if (!IsIndexReady)
            {
                // 1. Виводимо повідомлення про помилку через DialogService.
                _dialogService.ShowMessage(
                    "To perform an ordered search (TF-IDF), you must first build an inverted index of the corpus. Please go to the 'Manage' tab and perform indexing.",
                    "Error: Index not built");

                // 2. Скидаємо стан і повертаємось.
                _mainVM.CurrentStatusMessage = "Search aborted: Index required.";

                // Оскільки IsBusy ще не встановлено, це не призведе до NullReferenceException.
                return;
            }

            // Починаємо пошук (якщо індекс побудовано)

            // 1. Встановлюємо стан зайнятості для блокування UI та відображення прогресу.
            _mainVM.IsBusy = true;
            _mainVM.CurrentStatusMessage = $"Searching for: {query} (Ranked)...";

            // 2. Очищаємо колекцію результатів перед початком нового пошуку.
            _mainVM.SimilarResults.Clear();

            IEnumerable<SimilarityResult> searchResults = null;

            try
            {
                // 3. Викликаємо Model для обчислення ранжованих результатів у фоновому потоці (CPU-bound)
                await Task.Run(() =>
                {
                    // Model-логіка: Використовуємо IAnalyzer для обчислення косинусної подібності запиту.
                    searchResults = _analyzer.SearchByQuery(query, similarityThreshold: 0.01);
                });

                // 4. Оновлення UI/ViewModel-колекцій (після await, у UI-потоці)
                if (searchResults != null && searchResults.Any())
                {
                    // Додаємо всі ранжовані результати до колекції для відображення.
                    foreach (var result in searchResults)
                    {
                        _mainVM.SimilarResults.Add(result);
                    }

                    // Перемикаємо на вкладку результатів, щоб користувач їх побачив.
                    _mainVM.SelectedTabIndex = 1;

                    _mainVM.CurrentStatusMessage = $"Ranked search completed. Found {searchResults.Count()} matches. Ready.";
                }
                else
                {
                    // Якщо результатів немає, повідомляємо користувача через DialogService.
                    _dialogService.ShowMessage($"Ranked search for '{query}' found no matches.", "Search Results");
                    _mainVM.CurrentStatusMessage = "Ranked search completed. None found.";
                }
            }
            catch (Exception ex)
            {
                // Обробка критичних помилок під час обчислення у фоновому потоці.
                _dialogService.ShowMessage($"Error during ranked search: {ex.Message}", "Fatal Error");
                _mainVM.CurrentStatusMessage = "Error during ranked search.";
            }
            finally
            {
                // Вимикаємо індикатор зайнятості незалежно від результату.
                _mainVM.IsBusy = false;

                // Повертаємо логіку фільтрації ICollectionView до нормального стану.
                Documents.Filter = FilterDocuments;
            }
        }
    }
}