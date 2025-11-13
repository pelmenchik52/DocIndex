using KURSOVA.Model;
using KURSOVA.Model.Interfaces;
using KURSOVA.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Services
{
    // Контракт для служби, яка керує всіма взаємодіями з UI, 
    // ізолюючи ViewModel від залежностей System.Windows.
    public interface IDialogService
    {
        // 1. Стандартні діалоги

        // Повертає true/false залежно від натиснутої кнопки (Yes/No)
        bool ShowConfirmation(string message, string caption);

        // Показує інформаційне повідомлення користувачу
        void ShowMessage(string message, string caption);

        // 2. Файлові діалоги (для Import/Export)

        // Показує діалог відкриття файлу (для імпорту). Повертає обраний шлях або порожній рядок.
        string ShowOpenFileDialog(string filter);

        // Показує діалог збереження файлу (для експорту). Повертає шлях збереження або порожній рядок.
        string ShowSaveFileDialog(string filter, string defaultFileName);

        // 3. Специфічні вікна

        // Показує діалогове вікно для редагування документа.
        void ShowEditDocumentDialog(IDocument documentToEdit);

        // Показує модальне вікно налаштувань.
        // Приймає ViewModel для встановлення DataContext.
        void ShowSettingsWindow(SettingViewModel settingsVM);

        // Показує немодальне вікно довідки.
        void ShowHelpWindow();

        // Показує модальне вікно словника частот.
        // Приймає ViewModel, яка містить логіку завантаження даних.
        void ShowFrequencyDictionary(FrequencyViewModel frequencyVM);

        // Метод для Multiselect документів
        string[] ShowOpenMultiFileDialog(string filter);

        // Метод для вибору папки
        public string ShowFolderBrowserDialog(string description);

    }
}