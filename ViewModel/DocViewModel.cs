using KURSOVA.Model;
using System.Linq;
using System;
using KURSOVA.Model.Interfaces;

namespace KURSOVA.ViewModel
{
    // Обгортка (ViewModel Wrapper) для об'єкта даних IDocument.
    // Відповідає за представлення одного документа у списках (наприклад, ListBox, DataGrid).
    public class DocViewModel : ViewModelBase
    {
        private readonly IDocument _model;

        // Проксі-властивості (читання даних з Model) 

        // ID документа (для унікальної ідентифікації).
        public int Id => _model.Id;

        // Заголовок документа.
        public string Title => _model.Title;

        // Шлях до файлу.
        public string FilePath => _model.FilePath;

        // Обчислювана властивість: Підрахунок слів із повного тексту Model. 
        public int WordCount => _model.FullText.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Count();

        // Обчислювана властивість: Створення прев'ю (перші 150 символів) для UI.
        public string Preview => (string.IsNullOrEmpty(_model.FullText)
            ? "Empty"
            : _model.FullText.Substring(0, Math.Min(_model.FullText.Length, 150)).Replace("\n", " ") + "...");

        //  Властивості ViewModel (керування станом UI)

        private bool _isSelected;
        // Стан вибору у списку (наприклад, для елемента ListBox).
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                // Використовуємо успадкований OnPropertyChanged
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        // Конструктор: Отримує об'єкт Model (IDocument) для обгортки.
        public DocViewModel(IDocument model)
        {
            _model = model;
        }

        // Метод, що дозволяє іншим ViewModel (наприклад, MainViewModel) отримати доступ до базової Model.
        public IDocument GetModel() => _model;

        // Метод викликається після того, як Model було змінено (наприклад, після збереження редагування).
        public void RefreshProperties()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(WordCount));
            OnPropertyChanged(nameof(Preview));
        }
    }
}