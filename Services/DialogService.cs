using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows; 
using KURSOVA.Model.Interfaces; 
using KURSOVA.ViewModel;   
using KURSOVA.Model;         
using Microsoft.Win32;     
using KURSOVA.Services;
using KURSOVA.View;
using Ookii.Dialogs.Wpf;

namespace KURSOVA.Services
{
    // Реалізація IDialogService. Відповідає за всі взаємодії з UI, 
    // що включають модальні вікна та діалоги, ізолюючи ViewModel від System.Windows.
    public class DialogService : IDialogService
    {
        // Показує діалогове вікно підтвердження з повідомленням і заголовком.
        public bool ShowConfirmation(string message, string caption)
        {
            var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        // Показує інформаційне діалогове вікно з повідомленням і заголовком.
        public void ShowMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }


        // [1] Діалог Редагування (Модальний)
        public void ShowEditDocumentDialog(IDocument documentToEdit)
        {
            var editView = new EditDocumentView();
            Action closeAction = () => { editView.Close(); };

            // 1. Створюємо ViewModel (конструктор тепер безпечний)
            var editVM = new EditDocumentViewModel(documentToEdit, closeAction, this);

            try
            {
                editVM.LoadDocumentText();

                // 3. Якщо завантаження успішне, встановлюємо DataContext і показуємо вікно
                editView.DataContext = editVM;
                editView.ShowDialog();
            }
            catch (System.IO.IOException ex)
            {
                // 4. Обробляємо помилку (файл заблоковано)
                ShowMessage($"Failed to open document. It is being used by another process.\n\n{ex.Message}", "File Locked");
            }
            catch (Exception ex)
            {
                // 5. Обробка інших помилок
                ShowMessage($"An error occurred: {ex.Message}", "Error");
            }
        }

        // [2] Діалог Відкриття Файлу (для імпорту)
        // Повертає шлях до файлу або null/string.Empty.
        public string ShowOpenFileDialog(string filter)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = filter, // Наприклад, "Text files (*.txt)|*.txt"
                Multiselect = false
            };

            if (dlg.ShowDialog() == true)
            {
                return dlg.FileName;
            }
            return string.Empty;
        }

        // [3] Діалог Збереження Файлу (для експорту)
        // Повертає шлях, куди потрібно зберегти файл, або null/string.Empty.
        public string ShowSaveFileDialog(string filter, string defaultFileName)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Filter = filter, // Наприклад, "CSV|*.csv|Text|*.txt"
                FileName = defaultFileName
            };

            if (dlg.ShowDialog() == true)
            {
                return dlg.FileName;
            }
            return string.Empty;
        }

        // [4] Показ вікна Налаштувань (Модально)
        // Приймає готову ViewModel (SettingsVM) для встановлення DataContext.
        public void ShowSettingsWindow(SettingViewModel settingsVM)
        {
            SettingView settingsWindow = new SettingView();
            settingsWindow.DataContext = settingsVM;
            settingsWindow.ShowDialog();
        }

        // [5] Показ вікна Довідки (Немодально)
        public void ShowHelpWindow()
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show(); // Немодальне відображення
        }

        // [6] Показ вікна Словника Частот (Модально)
        // Потрібно для відображення словника частот.
        public void ShowFrequencyDictionary(FrequencyViewModel frequencyVM)
        {
            FrequencyView frequencyWindow = new FrequencyView();

            Action closeAction = () => { frequencyWindow.Close(); };

            // Встановлюємо робочий делегат у ViewModel
            frequencyVM.SetCloseAction(closeAction);

            // Метод LoadFrequencyDictionary має бути викликаний тут, 
            // щоб дані були завантажені перед показом вікна
            frequencyWindow.DataContext = frequencyVM;
            frequencyVM.LoadFrequencyDictionary();
            frequencyWindow.ShowDialog();
        }
        public string[] ShowOpenMultiFileDialog(string filter)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Multiselect = true, // Ключова відмінність
                Title = "Select files to import"
            };

            if (dlg.ShowDialog() == true)
            {
                return dlg.FileNames; // Повертає масив шляхів
            }
            return new string[0]; // Повертає порожній масив, якщо скасовано
        }

        // Діалог вибору папки
        public string ShowFolderBrowserDialog(string description)
        {
            // Ця View-логіка ізольована в шарі Сервісів.
            var dlg = new VistaFolderBrowserDialog
            {
                Description = description,
                UseDescriptionForTitle = true
            };

            if (dlg.ShowDialog() == true)
            {
                return dlg.SelectedPath;
            }
            return string.Empty;
        }

    }
}   