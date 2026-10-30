using DocIndex.Commands;
using DocIndex.Model;
using DocIndex.Model.Classes;
using DocIndex.Model.Interfaces;
using DocIndex.Services; 
using DocIndex.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DocIndex.ViewModel
{
    // Головний контекст додатку, агрегує всі дочірні ViewModel і керує загальним станом.
    public class MainViewModel : ViewModelBase
    {
        //  Приватні поля для сервісів UI. 
        private readonly IDialogService _dialogService;
        private readonly IThemeService _themeService;
        private readonly IIndexer _indexer;
        private readonly IAnalyzer _analyzer;
        private readonly IKMeansCluster _clusterer;


        // Властивості стану та колекцій

        public ObservableCollection<ClusterViewModel> ClusteringResults { get; } = new ObservableCollection<ClusterViewModel>();
        public ObservableCollection<SimilarityResult> SimilarResults { get; } = new ObservableCollection<SimilarityResult>();
        public ObservableCollection<IDocument> Documents { get; set; } // Прив'язка до Corpus.Instance.Documents

        // Обчислюваний метод для спрощення OnPropertyChanged
        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;   
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        //  Загальний Стан UI 
        private string _currentStatusMessage = "Done";
        public string CurrentStatusMessage
        {
            get => _currentStatusMessage;
            set => SetProperty(ref _currentStatusMessage, value);
        }

        private bool _isBusy = false;
        // Індикатор зайнятості (використовується для асинхронних операцій).
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private IDocument _selectedDocument;
        // Поточний вибраний документ (керує станом DocumentsVM).
        public IDocument SelectedDocument
        {
            get => _selectedDocument;
            set => SetProperty(ref _selectedDocument, value);
        }

        private int _selectedTabIndex;
        // Індекс поточної активної вкладки.
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        // Агреговані Дочірні ViewModel
        public DocumentsViewModel DocumentsVM { get; set; }
        public KeywordsViewModel KeywordsVM { get; set; }
        public StatsViewModel StatsVM { get; set; }
        public ManagementViewModel ManagementVM { get; set; }
        public SettingViewModel SettingsVM { get; set; } 

        // Команди 
        public ICommand btnSetting { get; set; }
        public ICommand btnExitApp { get; set; }
        public ICommand btnHelpMenu { get; set; }
        public ICommand btnImportFiles { get; set; }

        // Конструктор
        public MainViewModel(IDialogService dialogService, IThemeService themeService)
        {
            _dialogService = dialogService;
            _themeService = themeService;

            // Ініціалізація Шару Model
            _indexer = new Indexer();
            _analyzer = new Analyzer(_indexer);
            _clusterer = new KMeansCluster(_indexer);

            // Ініціалізація Дочірніх ViewModel
            DocumentsVM = new DocumentsViewModel(this, _dialogService, _analyzer);
            KeywordsVM = new KeywordsViewModel(_analyzer, _dialogService);
            StatsVM = new StatsViewModel(_indexer);
            SettingsVM = new SettingViewModel(_dialogService, _themeService, null);

            ManagementVM = new ManagementViewModel(this, _indexer, _analyzer, _clusterer, KeywordsVM, StatsVM, _dialogService, _themeService);

            Documents = Corpus.Instance.Documents;

            // Ініціалізація Команд
            btnSetting = new RelayCommand(OpenSettingsWindowAction, CanExecuteAction);
            btnExitApp = new RelayCommand(ExitApplicationAction, CanExecuteAction);
            btnHelpMenu = new RelayCommand(OpenHelpWindowAction, CanExecuteAction);
            btnImportFiles = new RelayCommand(ImportFilesAction, CanExecuteAction);
        }

        private bool CanExecuteAction(object obj)
        {
            return !IsBusy;
        }

        // Реалізації команд
        // Імпорт файлів
        private void ImportFilesAction(object obj)
        {
            string filePath = _dialogService.ShowOpenFileDialog("Text files (*.txt)|*.txt|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    // Логіка створення документа
                    IDocument newDoc = new Document(filePath);
                    Corpus.Instance.AddDocument(newDoc);
                    _dialogService.ShowMessage($"File '{System.IO.Path.GetFileName(filePath)}' imported succesfuly.", "Import");
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Error during opening file: {ex.Message}", "Error");
                }
            }
        }

        // Відкриття вікна налаштувань
        private void OpenSettingsWindowAction(object obj)
        {
            _dialogService.ShowSettingsWindow(SettingsVM);
        }

        // Вихід з додатку
        private void ExitApplicationAction(object obj)
        {
            Application.Current.Shutdown();
        }

        // Відкриття вікна допомоги
        private void OpenHelpWindowAction(object obj)
        {
            _dialogService.ShowHelpWindow();
        }
    }
}