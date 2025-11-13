using KURSOVA.Commands;
using KURSOVA.Model;
using KURSOVA.Model.Interfaces;
using KURSOVA.Services;
using System;
using System.ComponentModel;
using System.IO; 
using System.Windows.Input;

namespace KURSOVA.ViewModel
{
    // ViewModel для модального вікна редагування документа.
    public class EditDocumentViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        // Посилання на об'єкт Model, який редагується.
        public IDocument CurrentDocument { get; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        // Приватне поле для зберігання вмісту документа, який редагується користувачем.
        private string _documentText;

        // Властивість, прив'язана до TextBox у View (двосторонній біндінг).
        public string DocumentText
        {
            get { return _documentText; }
            set
            {
                if (_documentText != value)
                {
                    _documentText = value;
                    OnPropertyChanged(nameof(DocumentText));
                }
            }
        }

        // Команди для дій користувача (кнопки).
        public ICommand btnSaveDocument { get; }
        public ICommand CancelCommand { get; }

        // Абстрактна дія, яка буде виконувати закриття вікна (запобігає залежності від System.Windows.Window).
        private Action _closeWindowAction;

        // Отримує документ для редагування та колбек для закриття.
        public EditDocumentViewModel(IDocument documentToEdit, Action closeWindowAction, IDialogService dialogService)
        {
            CurrentDocument = documentToEdit;
            _closeWindowAction = closeWindowAction;
            _dialogService = dialogService;

            // Ініціалізація команд.
            btnSaveDocument = new RelayCommand(SaveChangesAction, CanExecute);
            CancelCommand = new RelayCommand(CancelEditAction, CanExecute);
        }
        public void LoadDocumentText()
        {
            DocumentText = File.ReadAllText(CurrentDocument.FilePath);
        }

        // Логіка CanExecute: тут завжди повертаємо true, оскільки дії завжди доступні.
        private bool CanExecute(object obj)
        {
            return true;
        }

        // Зберігає змінений текст назад у файл.
        private void SaveChangesAction(object obj)
        {
            try
            {
                // 1. Записуємо змінений текст назад у файл.
                File.WriteAllText(CurrentDocument.FilePath, DocumentText);

                // 2. Сповіщаємо Model про оновлення
                CurrentDocument.RefreshPreview();

                // 3. Закриваємо вікно (лише якщо збереження успішне).
                _closeWindowAction?.Invoke();
            }
            // Обробка винятку "Відмовлено в доступі" (Read-only)
            catch (System.UnauthorizedAccessException ex)
            {
                _dialogService.ShowMessage(
                    "Unable to save changes. Access denied.\n" +
                    "Make sure the file does not have the 'Read-only' attribute.",
                    "Access is denied");
            }
            // Обробка інших помилок I/O (напр. файл заблокований)
            catch (System.IO.IOException ex)
            {
                _dialogService.ShowMessage(
                    $"The file could not be saved (it may be locked by another application).\nDetails: {ex.Message}",
                    "Save error");
            }
            // Загальний обробник
            catch (Exception ex)
            {
                _dialogService.ShowMessage(
                    $"An unexpected error occurred.: {ex.Message}",
                    "Critical error");
            }
        }

        // Скасовує редагування (просто закриває вікно без збереження змін).
        private void CancelEditAction(object obj)
        {
            _closeWindowAction?.Invoke();
        }
    }
}