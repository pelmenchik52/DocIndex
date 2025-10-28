using KURSOVA.Commands;
using KURSOVA.Model;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace KURSOVA.ViewModel
{
    class EditDocumentViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public IDocument CurrentDocument { get; set; }

        private string _documentText;
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
        public ICommand btnSaveDocument { get; set; }
        public ICommand CancelCommand { get; set; }

        private Action _closeWindowAction;

        // --- Конструктор ---

        public EditDocumentViewModel(IDocument documentToEdit, Action closeWindowAction)
        {
            CurrentDocument = documentToEdit;
            _closeWindowAction = closeWindowAction; 
     
            DocumentText = File.ReadAllText(CurrentDocument.FilePath);

  
            btnSaveDocument = new RelayCommand(SaveChangesAction, CanExecute);
            CancelCommand = new RelayCommand(CancelEditAction, CanExecute);
        }

        private bool CanExecute(object obj)
        {
            return true; 
        }

        private void SaveChangesAction(object obj)
        {
            File.WriteAllText(CurrentDocument.FilePath, DocumentText);

            CurrentDocument.RefreshPreview();

            _closeWindowAction?.Invoke();
        }

        private void CancelEditAction(object obj)
        {
            _closeWindowAction?.Invoke();
        }
    }
}