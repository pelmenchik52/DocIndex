using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KURSOVA.Model;

namespace KURSOVA.Model
{
    public class Document : IDocument, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _title;
        private string _preview;
        private long _wordCount;
        private DateTime _importDate;
        private string _filePath;


        //імя документу та вміст
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }
        public string Preview
        {
            get { return _preview; }
            // 'set' викликає OnPropertyChanged, сповіщаючи ListBox
            set { _preview = value; OnPropertyChanged(nameof(Preview)); }
        }

        public string FullText { get; private set; }

        //кількість слів та час імпорту документа
        public long WordCount
        {
            get { return _wordCount; }
            // 'set' викликає OnPropertyChanged, сповіщаючи ListBox
            set { _wordCount = value; OnPropertyChanged(nameof(WordCount)); }
        }
        public DateTime importDate
        {
            get { return _importDate; }
            set { _importDate = value; OnPropertyChanged(nameof(importDate)); }
        }

        //шлях до файлу
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        //конструктор
        public Document(string path)
        {
            this.FilePath = path; 
            this.Title = System.IO.Path.GetFileName(path);
            this.importDate = DateTime.Now;

   
            (this as IDocument).RefreshPreview();
        }

        void IDocument.RefreshPreview()
        {
            try
            {
                this.FullText = File.ReadAllText(this.FilePath);

                // 2. Рахуємо слова ВЖЕ З ПАМ'ЯТІ
                char[] separators = new char[] { ' ', '\r', '\n', '\t' };
                long count = this.FullText.Split(separators, StringSplitOptions.RemoveEmptyEntries).Length;

                // 3. Робимо Preview ВЖЕ З ПАМ'ЯТІ
                string previewText = this.FullText.Length > 150
                    ? this.FullText.Substring(0, 150) + "..."
                    : this.FullText;

                this.WordCount = count;
                this.Preview = previewText.Replace("\n", " ").Replace("\r", " ");
            }
            catch (Exception ex)
            {
                this.Preview = $"ПОМИЛКА: {ex.Message}";
                this.WordCount = 0;
            }

        }
    }
}
