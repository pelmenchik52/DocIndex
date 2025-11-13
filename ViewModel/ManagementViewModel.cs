using KURSOVA.Commands;
using KURSOVA.Model;
using KURSOVA.Model.Classes;
using KURSOVA.Model.Interfaces;
using KURSOVA.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Text.Json;
using System.IO;
using KURSOVA.Properties.Localizations;
using KURSOVA.Localization;
using System.Resources;

namespace KURSOVA.ViewModel
{
    // Керує основними операціями застосунку (Індексація, Кластеризація, I/O).
    // По суті це меню порграми, де є реалізації всіх необхідих кнопкок
    public class ManagementViewModel : ViewModelBase
    {
        // Залежності та сервіси 
        private readonly MainViewModel _mainVM;
        private readonly IIndexer _indexer;
        private readonly IAnalyzer _analyzer;
        private readonly IKMeansCluster _clusterer;
        private readonly KeywordsViewModel _keywordsVM;
        private readonly StatsViewModel _statsVM;
        private readonly IDialogService _dialogService;
        private readonly IThemeService _themeService;

        // Команди
        public ICommand btnBuildIndex { get; }
        public ICommand BtnFrequencyDictionary { get; }
        public ICommand btnSaveCorpus { get; }
        public ICommand btnImportFiles { get; }
        public ICommand btnImportFolder { get; }
        public ICommand BtnFindSimmilar { get; }
        public ICommand BtnClear { get; }
        public ICommand BtnClasterization { get; }
        public ICommand btnLoadCorpus { get; }

        public StatsViewModel StatsVM => _statsVM;

        // Стани

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                (btnBuildIndex as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _indexStatus;
        public string IndexStatus
        {
            get => _indexStatus;
            set { _indexStatus = value; OnPropertyChanged(nameof(IndexStatus)); }
        }

        private int _clusterCount = 2;
        public int ClusterCount
        {
            get => _clusterCount;
            set { _clusterCount = value; OnPropertyChanged(nameof(ClusterCount)); }
        }

        private bool _isIndexBuilt = false;
        public bool IsIndexBuilt
        {
            get => _isIndexBuilt;
            private set
            {
                _isIndexBuilt = value;
                OnPropertyChanged(nameof(IsIndexBuilt));
                // Оновлюємо кнопки, які залежать від індексу
                (btnBuildIndex as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        // Конструктор
        public ManagementViewModel(MainViewModel mainVM, IIndexer indexer, IAnalyzer analyzer,
                                   IKMeansCluster clusterer,
                                   KeywordsViewModel keywordsVM, StatsViewModel statsVM,
                                   IDialogService dialogService, IThemeService themeService)
        {
            // Зберігаємо залежності
            _mainVM = mainVM;
            _indexer = indexer;
            _analyzer = analyzer;
            _keywordsVM = keywordsVM;
            _statsVM = statsVM;
            _clusterer = clusterer;
            _dialogService = dialogService;
            _themeService = themeService;

            IndexStatus = Properties.Localizations.Resources.IndexStatus_NotBuilt;

            // Ініціалізація команд
            BtnFrequencyDictionary = new RelayCommand(OnShowFrequencyDictionary, CanExecuteIndexDependent);
            btnSaveCorpus = new RelayCommand(ExecuteSaveCorpus, CanExecuteIndexDependent);
            btnImportFiles = new RelayCommand(async (o) => await ImportFilesActionAsync(o), (o) => !IsBusy);
            BtnFindSimmilar = new RelayCommand(async (o) => await ExecuteFindSimilarAsync(o), CanExecuteFindSimilar);
            btnImportFolder = new RelayCommand(async (o) => await ImportFolderActionAsync(o), (o) => !IsBusy);
            btnBuildIndex = new RelayCommand(async (o) => await OnBuildIndexAsync(o), CanBuildIndex);
            btnLoadCorpus = new RelayCommand(async (o) => await ExecuteLoadCorpusAsync(o), (o) => !IsBusy);
            BtnClear = new RelayCommand(async (o) => await ExecuteClearAsync(o), CanBuildIndex);
            BtnClasterization = new RelayCommand(async (o) => await ExecuteClasterizationAsync(o), CanExecuteIndexDependent);

            // Підписки
            Corpus.Instance.Documents.CollectionChanged += OnCorpusChanged;
            _mainVM.PropertyChanged += OnMainVmPropertyChanged;
        }

        // Логіка CanExecute
        private bool CanBuildIndex(object obj) => Corpus.Instance.Documents.Any() && !IsBusy;
        private bool CanExecuteIndexDependent(object obj) => IsIndexBuilt && !IsBusy;
        private bool CanExecuteFindSimilar(object obj) => IsIndexBuilt && _mainVM.SelectedDocument != null && !IsBusy;

        // Логіка команд

        // Викликається при зміні корпусу (додавання/видалення документів)
        private void OnCorpusChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Скидаємо індекс, оскільки корпус змінився
            IsIndexBuilt = false;

            // Оновлюємо кнопки.
            (btnBuildIndex as RelayCommand)?.RaiseCanExecuteChanged();
            (BtnFrequencyDictionary as RelayCommand)?.RaiseCanExecuteChanged();
            (BtnFindSimmilar as RelayCommand)?.RaiseCanExecuteChanged();
            (BtnClear as RelayCommand)?.RaiseCanExecuteChanged();
        }

        // Обробник зміни SelectedDocument у MainViewModel
        private void OnMainVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedDocument))
            {
                (BtnFindSimmilar as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // [1] Імпорт файлів
        private async Task ImportFilesActionAsync(object obj)
        {
            // Використовуємо сервіс для OpenFileDialog Multiselect
            string[] fileNames = _dialogService.ShowOpenMultiFileDialog("Text files (*.txt)|*.txt|All files (*.*)|*.*");

            if (fileNames.Length == 0)
            {
                _mainVM.CurrentStatusMessage = "File import cancelled.";
                return;
            }

            _mainVM.IsBusy = true;
            _mainVM.CurrentStatusMessage = "Loading of files...";
            LogMessages.Add("Importing of files...");

            int successCount = 0;

            try
            {
                // ... (Task.Run логіка I/O та оновлення Corpus залишається без змін) ...
                await Task.Run(() =>
                {
                    foreach (string filename in fileNames)
                    {
                        try
                        {
                            IDocument newDoc = new Document(filename);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Corpus.Instance.AddDocument(newDoc);
                                _mainVM.CurrentStatusMessage = $"Loaded: {System.IO.Path.GetFileName(filename)}";
                                LogMessages.Add($"File added: {System.IO.Path.GetFileName(filename)}");
                            });
                            successCount++;
                        }
                        catch (System.IO.IOException ioEx)
                        {
                            // Обробка помилки I/O (наприклад, "файл використовується іншою програмою").
                            LogMessages.Insert(0, $"Error loading corpus (I/O): {ioEx.Message}");

                            // Виведення повідомлення про помилку через сервіс.
                            _dialogService.ShowMessage(
                                $"An I/O error occurred while reading the file. Please ensure the file is not open in another program.\n\nDetails: {ioEx.Message}",
                                "File Access Error");

                            _mainVM.CurrentStatusMessage = "Error loading corpus: File locked or inaccessible.";
                            LogMessages.Add("Error loading corpus: " + ioEx.Message);
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LogMessages.Add($"Error adding file: {filename} - {ex.Message}");
                            });
                        }
                    }
                });

                _mainVM.CurrentStatusMessage = $"Import completed. Successfully added {successCount} documents. Ready.";
                LogMessages.Add($"Import completed. Successfully added {successCount} documents.");
            }
            catch (System.IO.IOException ioEx)
{
    // Обробка помилки I/O (наприклад, "файл використовується іншою програмою").
    LogMessages.Insert(0, $"Error loading corpus (I/O): {ioEx.Message}");
    
    // Виведення повідомлення про помилку через сервіс.
    _dialogService.ShowMessage(
        $"An I/O error occurred while reading the file. Please ensure the file is not open in another program.\n\nDetails: {ioEx.Message}", 
        "File Access Error");
    
    _mainVM.CurrentStatusMessage = "Error loading corpus: File locked or inaccessible.";
    LogMessages.Add("Error loading corpus: " + ioEx.Message);
}
            catch (Exception ex)
            {
                _mainVM.CurrentStatusMessage = $"Fatal error during file import: {ex.Message}";
                LogMessages.Add("Fatal error during file import: " + ex.Message);
                _dialogService.ShowMessage($"A fatal error occured during import: {ex.Message}", "Error");
            }
            finally
            {
                _mainVM.IsBusy = false;
            }
        }

        // [2] Імпорт папки
        private async Task ImportFolderActionAsync(object obj)
        {
            string folderPath = _dialogService.ShowFolderBrowserDialog("Chose directory with txt documents");

            // 1. Перевірка на скасування діалогу.
            if (string.IsNullOrEmpty(folderPath))
            {
                _mainVM.CurrentStatusMessage = "Folder import cancelled.";
                return;
            }

            // Встановлюємо індикатори прогресу та статусу.
            _mainVM.IsBusy = true;
            _mainVM.CurrentStatusMessage = "Loading of files from folder...";
            LogMessages.Add("Importing files from folder...");

            int successCount = 0;

            try
            {
                // 2. переносимо всю важку роботу (I/O) у фоновий потік
                await Task.Run(() =>
                {
                    // Отримання списку файлів (I/O операція)
                    string[] files = System.IO.Directory.GetFiles(folderPath, "*.txt", System.IO.SearchOption.TopDirectoryOnly);

                    foreach (string file in files)
                    {
                        try
                        {
                            // Створення документа та читання його вмісту.
                            IDocument newDoc = new Document(file);

                            // 3. Оновлення UI/Corpus: Виконуємо через Dispatcher.Invoke.
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Corpus.Instance.AddDocument(newDoc);
                                _mainVM.CurrentStatusMessage = $"Loaded: {System.IO.Path.GetFileName(file)}";
                                LogMessages.Add($"File added: {System.IO.Path.GetFileName(file)}");
                            });

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            // Логування помилки додавання конкретного файлу.
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                LogMessages.Add($"Error adding file: {file} - {ex.Message}");
                            });
                        }
                    }
                });

                // 4. Фінальне повідомлення про успіх (повертаємось у UI-потік).
                _mainVM.CurrentStatusMessage = $"Import completed. Successfully added {successCount} documents. Ready.";
                LogMessages.Add($"Import completed. Successfully added {successCount} documents from folder: {folderPath}");
            }
            catch (Exception ex)
            {
                // 5. Обробка критичних помилок.
                string errorMessage = $"A fatal error during importing folder: {folderPath}\n{ex.Message}";

                _dialogService.ShowMessage(errorMessage, "Error");

                _mainVM.CurrentStatusMessage = "Error loading files from folder.";
                LogMessages.Add("Error importing files from folder: " + folderPath + " - " + ex.Message);
            }
            finally
            {
                // 6. Вимикаємо індикатор зайнятості незалежно від результату.
                _mainVM.IsBusy = false;
            }
        }

        // [3] Метод, який запускає побудову інвертованого індексу корпусу (Indexer).
        private async Task OnBuildIndexAsync(object obj)
        {
            // Встановлюємо загальний індикатор зайнятості.
            _mainVM.IsBusy = true;
            _mainVM.CurrentStatusMessage = "Building the corpus index...";
            LogMessages.Add("Building the corpus index...");

            int uniqueWordCount = 0;

            try
            {
                // 1. Зміна статусу індексу (для відображення у UI)
                IndexStatus = Properties.Localizations.Resources.IndexStatus_Building;

                // 2. переносимо важку роботу (обчислення TF-IDF) у фоновий потік
                await Task.Run(() =>
                {
                    // Виклик Model-сервісу: Індексація корпусу (CPU-bound операція).
                    _indexer.IndexCorpus(Corpus.Instance.Documents);
                    // Отримуємо результат індексації у фоновому потоці.
                    uniqueWordCount = _indexer.GetVocabulary().Count;
                });

                // 3. Оновлення стану після успішного завершення (повертаємось у UI-потік)
                IndexStatus = Properties.Localizations.Resources.IndexStatus_Built;

                // Оновлюємо дочірні ViewModel з новими обчисленими даними
                _keywordsVM.LoadKeywordsAction();
                _statsVM.LoadStats();

                IsIndexBuilt = true; // Встановлюємо прапор, що індекс готовий.

                LogMessages.Add($"Indexing is completed. Founded {uniqueWordCount} unique words.");

                // Перемикаємо на вкладку, де відображаються результати аналізу.
                _mainVM.SelectedTabIndex = 3;
                _mainVM.CurrentStatusMessage = "Indexing is completed. Ready.";
                LogMessages.Add("Indexing completed successfully.");
            }
            catch (Exception ex)
            {
                // Обробка помилок під час фонової операції.
                IsIndexBuilt = false;
                _mainVM.CurrentStatusMessage = "Error during indexing.";
                LogMessages.Add("Error of indexing: " + ex.Message);

                // Використовуємо сервіс для відображення критичної помилки (чистий MVVM).
                _dialogService.ShowMessage($"A fatal error occured during indexing: {ex.Message}", "Error");
            }
            finally
            {
                // 4. Вимикаємо індикатор зайнятості незалежно від результату.
                _mainVM.IsBusy = false;
            }
        }

        // [4] Пошук подібності 
        private async Task ExecuteFindSimilarAsync(object obj)
        {
            // Перевірка виконується в UI-потоці: отримуємо цільовий документ.
            var targetDoc = _mainVM.SelectedDocument;
            if (targetDoc == null)
            {
                _mainVM.CurrentStatusMessage = "No document selected for comparison.";
                return;
            }

            _mainVM.IsBusy = true; // Вмикаємо індикатор зайнятості
            _mainVM.CurrentStatusMessage = "Finding similar documents...";
            LogMessages.Add("Finding similar documents...");

            // Змінна для зберігання результатів з фонового потоку.
            IEnumerable<SimilarityResult> results = null;

            try
            {
                LogMessages.Add($"Finding similar to: {targetDoc.Title}...");

                // Очищаємо колекцію результатів у UI-потоці.
                _mainVM.SimilarResults.Clear();

                // 1. переносимо обчислення (cpu-bound) у фоновий потік.
                await Task.Run(() =>
                {
                    // Викликаємо Model: обчислення косинусної подібності.
                    results = _analyzer.FindSimilarDocuments(targetDoc, 0.1);
                });

                // 2. Оновлення UI/ViewModel-колекцій (після await, у UI-потоці).
                if (results != null && results.Any())
                {
                    foreach (var res in results)
                    {
                        // Додавання до ObservableCollection.
                        _mainVM.SimilarResults.Add(res);
                    }
                    LogMessages.Add($"Founded {results.Count()} similar documents.");

                    // Зміна вкладки на вкладку результатів.
                    _mainVM.SelectedTabIndex = 1;

                    _mainVM.CurrentStatusMessage = $"Similar documents search completed. Found {results.Count()} documents. Ready.";
                    LogMessages.Add("Similar documents search completed successfully.");
                }
                else
                {
                    // Якщо результатів немає.
                    _mainVM.IsBusy = false;

                    string message = Localization.LocExtension.GetLocalizedValue("SimilarDocumentNotFound_Text");
                    string title = Localization.LocExtension.GetLocalizedValue("Information_Title");
                    _dialogService.ShowMessage(message, title);

                    _mainVM.CurrentStatusMessage = "Similar documents search completed. None found.";
                    LogMessages.Add("Similar documents is not found.");
                }
            }
            catch (Exception ex)
            {
                // Обробка помилок під час обчислення у фоновому потоці.
                _mainVM.CurrentStatusMessage = "Error during similar documents search.";
                LogMessages.Add("Error during similar documents search: " + ex.Message);
                _dialogService.ShowMessage($"A fatal error occured during search: {ex.Message}", "Error");
            }
            finally
            {
                // Вимикаємо індикатор зайнятості незалежно від результату.
                _mainVM.IsBusy = false;
            }
        }

        // [5] Очищення
        private async Task ExecuteClearAsync(object obj)
        {
            // Використовуємо сервіс для підтвердження
            var result = _dialogService.ShowConfirmation(
                "Are you sure you want to clear all fields?",
                "Clearing confirmation");

            if (!result)
            {
                _mainVM.CurrentStatusMessage = "Clearing cancelled.";
                return;
            }

            _mainVM.IsBusy = true;
            _mainVM.CurrentStatusMessage = "Clearing corpus and index...";
            LogMessages.Add("Clearing corpus and index...");

            try
            {
                await Task.Run(() =>
                {
                    // Логіка Model
                    Corpus.Instance.RemoveAllDocuments();
                    _indexer.ClearIndex();
                });

                IsIndexBuilt = false;

                LogMessages.Clear();
                _mainVM.SimilarResults.Clear();
                _mainVM.ClusteringResults.Clear();

                _statsVM.LoadStats();
                _keywordsVM.LoadKeywordsAction();

                _mainVM.CurrentStatusMessage = "Corpus and index cleared. Ready.";
                LogMessages.Add("Corpus and index cleared successfully.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Error during cleaning: {ex.Message}", "Error");
                _mainVM.CurrentStatusMessage = "Error during clearing corpus and index.";
                LogMessages.Add("Error during clearing: " + ex.Message);
            }
            finally
            {
                _mainVM.IsBusy = false;
            }
        }

        // [6] Показ словника частот 
        private void OnShowFrequencyDictionary(object obj)
        {
            // 1. Створюємо ViewModel
            var freqVM = new FrequencyViewModel(_indexer, null, _dialogService);

            // 2. Делегуємо показ вікна сервісу
            _dialogService.ShowFrequencyDictionary(freqVM);
        }

        // [7] Збереження корпусу
        private void ExecuteSaveCorpus(object obj)
        {
            string filePath = _dialogService.ShowSaveFileDialog(
                 filter: "Corpus File (*.json)|*.json",
                 defaultFileName: "my_corpus.json"
            );

            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                // Вмикаємо Progress Bar
                _mainVM.IsBusy = true;
                _mainVM.CurrentStatusMessage = "Saving corpus...";
                LogMessages.Add("Saving corpus...");

                // Логіка збереження
                var filePaths = Corpus.Instance.Documents.Select(doc => doc.FilePath).ToList();
                string json = JsonSerializer.Serialize(filePaths, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);

                _mainVM.CurrentStatusMessage = "Corpus saved successfully.";
                LogMessages.Add("Corpus saved successfully.");

                _dialogService.ShowMessage("Corpus saved successfully!", "Save");
            }
            catch (Exception ex)
            {
                LogMessages.Insert(0, $"Error of saving corpus: {ex.Message}");
                _dialogService.ShowMessage($"Error of saving: {ex.Message}", "Error");
                _mainVM.CurrentStatusMessage = "Error saving corpus.";
                LogMessages.Add("Error saving corpus: " + ex.Message);
            }
            finally
            {
                _mainVM.IsBusy = false;
                if (_mainVM.CurrentStatusMessage.Contains("Corpus saved successfully"))
                {
                    _mainVM.CurrentStatusMessage = "Ready";
                }
            }
        }

        // [8] Завантаження корпусу
        private async Task ExecuteLoadCorpusAsync(object obj)
        {
            // Використовуємо сервіс діалогів для вибору JSON-файлу корпусу.
            string filePath = _dialogService.ShowOpenFileDialog("Corpus File (*.json)|*.json");

            // Якщо користувач скасував діалог, виходимо.
            if (string.IsNullOrEmpty(filePath)) return;

            // Встановлюємо прапор зайнятості для блокування UI та відображення прогресу.
            _mainVM.IsBusy = true;
            this.IsBusy = true; // Блокування кнопок у поточному ViewModel
            _mainVM.CurrentStatusMessage = "Loading corpus data...";

            LogMessages.Insert(0, "Loading the corpus...");

            try
            {
                List<string> filePaths = null;

                // Читання JSON-файлу та десеріалізація шляхів виконується у фоновому потоці (I/O).
                await Task.Run(() =>
                {
                    string json = File.ReadAllText(filePath);
                    // JSON-файл містить список шляхів до оригінальних текстових файлів.
                    filePaths = JsonSerializer.Deserialize<List<string>>(json);
                });

                // Логіка підтвердження очищення (виконується у UI-потоці)
                if (Corpus.Instance.Documents.Any())
                {
                    // Використовуємо сервіс для отримання підтвердження від користувача.
                    var confirmed = _dialogService.ShowConfirmation("Clear current corpus before loading?", "Loading the corpus");
                    if (confirmed)
                    {
                        // Очищення поточного корпусу та індексу (викликається асинхронний метод).
                        await ExecuteClearAsync(null);
                    }
                }

                // Логіка завантаження файлів
                int loadedCount = 0;
                List<string> missingFilesLog = new List<string>();
                if (filePaths != null)
                {
                    foreach (string path in filePaths)
                    {
                        // Перевіряємо, чи існує оригінальний файл за збереженим шляхом.
                        try
                        {
                            // Конструктор Document кине виняток, якщо файл не знайдено 
                            // (за умови, що ви прибрали try-catch з конструктора Document).
                            IDocument newDoc = new Document(path);

                            Corpus.Instance.AddDocument(newDoc);
                            loadedCount++;
                        }
                        // [ОБРОБКА ВИНЯТКУ] Ловимо помилку, якщо файл не знайдено.
                        catch (System.IO.FileNotFoundException ex)
                        {
                            string missingFileName = Path.GetFileName(path);
                            missingFilesLog.Add(missingFileName);
                            LogMessages.Insert(0, $"Error (Not Found): {missingFileName}");
                        }
                    }
                }

                // Оновлення статусу успіху
                LogMessages.Insert(0, $"Succesfuly loaded {loadedCount} documents.");
                _mainVM.CurrentStatusMessage = "Corpus loaded successfully.";
                LogMessages.Add("Corpus loaded successfully.");

            }
            catch (JsonException jsonEx) // Обробка пошкодженого JSON
            {
                // Обробка помилок, якщо файл .json пошкоджений або має неправильний формат.
                LogMessages.Insert(0, $"Error loading corpus: Invalid JSON format. {jsonEx.Message}");

                // Виведення повідомлення про помилку через сервіс.
                _dialogService.ShowMessage($"Failed to read corpus file. The file appears to be corrupted (Invalid JSON).\n{jsonEx.Message}", "Error loading corpus");

                _mainVM.CurrentStatusMessage = "Error loading corpus: Invalid file format.";
                LogMessages.Add("Error loading corpus: Invalid JSON format.");
            }
            catch (Exception ex)
            {
                // Обробка помилок під час десеріалізації або читання файлу корпусу.
                LogMessages.Insert(0, $"Error of loading corpus: {ex.Message}");
                // Виведення повідомлення про помилку через сервіс.
                _dialogService.ShowMessage($"Error of loading: {ex.Message}", "Error");
                _mainVM.CurrentStatusMessage = "Error loading corpus.";
                LogMessages.Add("Error loading corpus: " + ex.Message);
            }
            finally
            {
                // Зняття блокування UI незалежно від результату.
                this.IsBusy = false;
                _mainVM.IsBusy = false;
            }
        }

        // [9] Кластеризація
        private async Task ExecuteClasterizationAsync(object obj)
        {
            int documentCount = Corpus.Instance.Documents.Count;


            // 1. Перевірка ініціалізації 
            if (_mainVM.ClusteringResults == null)
            {
                _dialogService.ShowMessage(
                    "ClusteringResults is not initialized. Check MainViewModel constructor.",
                    "Clustering Error");
                _mainVM.CurrentStatusMessage = "Clustering error: ViewModel not initialized.";
                return;
            }

            // 2. Перевірка валідації
            if (ClusterCount > documentCount)
            {
                // Отримуємо локалізовані шаблони через LocExtension, а потім використовуємо _dialogService.
                string errorTitle = Localization.LocExtension.GetLocalizedValue("Clustering_Error_Title");
                string errorMessageTemplate = Localization.LocExtension.GetLocalizedValue("Clustering_Error_KTooLarge");

                // Форматуємо повідомлення: {0} = ClusterCount, {1} = documentCount
                string errorMessage = string.Format(
                    errorMessageTemplate,
                    ClusterCount,
                    documentCount
                );

                _dialogService.ShowMessage(errorMessage, errorTitle);

                _mainVM.CurrentStatusMessage = Localization.LocExtension.GetLocalizedValue("Clustering_Status_ValidationFailed");
                LogMessages.Add($"Clustering failed: K ({ClusterCount}) > Documents ({documentCount}).");
                return;
            }

            // Початок виконання 
            _mainVM.IsBusy = true;
            _mainVM.CurrentStatusMessage = $"Executing clasterization (k={ClusterCount})...";
            LogMessages.Add($"Executing clasterization (k={ClusterCount})...");

            _mainVM.ClusteringResults.Clear();
            IEnumerable<Cluster> modelResults = null;

            try
            {
                // 1. переносимо важку обчислювальну роботу у фоновий потік
                await Task.Run(() =>
                {
                    // Model-логіка, яка обчислюється асинхронно
                    modelResults = _clusterer.Cluster(ClusterCount, maxIterations: 100);
                });

                // 2. Оновлення UI/ViewModel-колекцій (після await, у UI-потоці)
                int clusterCount = 0;
                if (modelResults != null)
                {
                    foreach (var modelCluster in modelResults.OrderBy(c => c.ClusterId))
                    {
                        var clusterVM = new ClusterViewModel { ClusterId = modelCluster.ClusterId };

                        // Заповнення DocViewModel для кожного документа в кластері
                        foreach (var docId in modelCluster.DocumentIds)
                        {
                            var doc = Corpus.Instance.GetDocumentById(docId);
                            if (doc != null)
                            {
                                clusterVM.Documents.Add(new DocViewModel(doc));
                            }
                        }
                        _mainVM.ClusteringResults.Add(clusterVM);
                        clusterCount++;
                    }
                }

                // 3. Фінальне сповіщення
                LogMessages.Insert(0, $"Clasterization is done. Founded {clusterCount} clusters.");
                _mainVM.SelectedTabIndex = 2; // Перехід на вкладку результатів

                _mainVM.CurrentStatusMessage = $"Clasterization completed. Found {clusterCount} clusters. Ready.";
                LogMessages.Add("Clasterization completed successfully.");
            }
            catch (Exception ex)
            {
                // Обробка критичної помилки обчислення
                LogMessages.Insert(0, $"Error of clasterization: {ex.Message}");
                _mainVM.CurrentStatusMessage = "Error during clasterization.";
                LogMessages.Add("Error during clasterization: " + ex.Message);

                _dialogService.ShowMessage($"Fatal error during clasterization: {ex.Message}", "Critical Error");
            }
            finally
            {
                // 4. Вимикаємо Progress Bar
                _mainVM.IsBusy = false;
            }
        }
    }
}