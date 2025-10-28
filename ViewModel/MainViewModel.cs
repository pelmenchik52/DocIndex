using KURSOVA.Commands;
using KURSOVA.Model;
using KURSOVA.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KURSOVA.ViewModel
{
    public class MainViewModel
    {

        public ObservableCollection<IDocument> Documents { get; set; }

        public DocumentsViewModel DocumentsVM { get; set; }
        public KeywordsViewModel KeywordsVM { get; set; }
        public ICommand btnImportFiles { get; set; }
        public ICommand btnImportFolder { get; set; }
        public ICommand btnSetting {  get; set; }
        public ICommand btnExitApp { get; set; }
        public ICommand btnHelpMenu { get; set; }



        public MainViewModel()
        {
            DocumentsVM = new DocumentsViewModel();
            KeywordsVM = new KeywordsViewModel();

            Documents = Corpus.Instance.Documents;
            btnImportFiles = new RelayCommand(ImportFilesAction, CanShowWindow);
            btnImportFolder = new RelayCommand(ImportFolderAction, CanShowWindow);
            btnSetting = new RelayCommand(OpenSettingsWindowAction, CanShowWindow);
            btnExitApp = new RelayCommand(ExitApplicationAction, CanShowWindow);
            btnHelpMenu = new RelayCommand(OpenHelpWindowAction, CanShowWindow);
 
        }

        private bool CanShowWindow(object obj)
        {
            return true;
        }

        private void ImportFilesAction(object obj)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)

            {
                string documentPath = dlg.FileName;
                try
                {
                    IDocument newDoc = new Document(dlg.FileName);

                    Corpus.Instance.AddDocument(newDoc);

                }
                catch (Exception ex)
                {
                    {
                        MessageBox.Show("Помилка відкриття файлу", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
            }
        }


        private void ImportFolderAction(object obj)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dlg.Description = "Виберіть папку з текстовими документами";
            dlg.UseDescriptionForTitle = true;

            if (dlg.ShowDialog() == true)
            {
                string folderPath = dlg.SelectedPath;
                try
                {
                    // Отримуємо всі .txt файли з папки (можна додати інші розширення)
                    string[] files = System.IO.Directory.GetFiles(folderPath, "*.txt", System.IO.SearchOption.TopDirectoryOnly); // SearchOption.AllDirectories - для підпапок

                    foreach (string file in files)
                    {
                        try // Додамо try-catch для кожного файлу окремо
                        {
                            IDocument newDoc = new Document(file);
                            Corpus.Instance.AddDocument(newDoc);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Помилка додавання файлу: {file}\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // Продовжуємо обробку інших файлів
                        }
                    }
                   

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка доступу до папки: {folderPath}\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OpenSettingsWindowAction(object obj)
        {
            SettingView settingsWindow = new SettingView();

            
            settingsWindow.DataContext = new SettingViewModel();
           
            settingsWindow.ShowDialog();
        }
        private void ExitApplicationAction(object obj)
        {
            Application.Current.Shutdown();
        }
        private void OpenHelpWindowAction(object obj)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }

       

    }
}
