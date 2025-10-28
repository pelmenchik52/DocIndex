using KURSOVA.Commands;
using KURSOVA.Model;
using KURSOVA.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace KURSOVA.ViewModel
{
    public class DocumentsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand btnDeleteDocument { get; set; }
        public ICommand btnEditDocument { get; set; }


        public DocumentsViewModel()
        {
            Documents = Corpus.Instance.Documents;

            btnDeleteDocument = new RelayCommand(DeleteDocumentAction, CanShowWindow);
            btnEditDocument = new RelayCommand(EditDocumentAction, CanShowWindow);
        }





        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<IDocument> Documents { get; set; }

        private IDocument _selectedDocument;
        public IDocument SelectedDocument
        {
            get { return _selectedDocument; }
            set
            {
                if (_selectedDocument != value)
                {
                    _selectedDocument = value;
                    OnPropertyChanged(nameof(SelectedDocument));
                    IsDocumentSelected = (_selectedDocument != null);
                }
            }
        }

        private bool _isDocumentSelected;
        public bool IsDocumentSelected
        {
            get { return _isDocumentSelected; }
            private set
            {
                _isDocumentSelected = value;
                OnPropertyChanged(nameof(IsDocumentSelected));
            }
        }
 
        private bool CanShowWindow(object obj)
        {
            return true;
        }


        private void DeleteDocumentAction(object obj)
        {
            if (SelectedDocument != null)
            {
                Corpus.Instance.RemoveDocument(SelectedDocument);
            }
        }
        private void EditDocumentAction(object obj)
        {
            var editView = new EditDocumentView();
            var editVM = new EditDocumentViewModel(SelectedDocument, () => 
            {
                editView.Close(); 
            });

            editView.DataContext = editVM;

            editView.ShowDialog();
        }

    }
}