using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DocIndex.ViewModel;

namespace DocIndex.Model
{
    // Клас Document: зберігає дані документа та підтримує сповіщення про зміну властивостей.
    public class Document : ModelBase, IDocument 
    {

        // Поле для потокобезпечного генерування унікального ID.
        private static int nextId = 0;

        // Ідентифікатор документа. init гарантує, що він встановлюється лише в конструкторі.
        public int Id { get; init; }

        // Приватні поля для зберігання даних.
        private string title;
        private string preview;
        private long wordCount;
        private string _filePath;


        // Ім'я документу.
        public string Title
        {
            get { return title; }
            set { title = value; OnPropertyChanged(nameof(Title)); }
        }

        // Короткий вміст для відображення у списку.
        public string Preview
        {
            get { return preview; }
            set { preview = value; OnPropertyChanged(nameof(Preview)); }
        }

        // Повний вміст документа. Встановлюється лише під час завантаження.
        public string FullText { get; private set; }

        // Кількість слів у документі.
        public long WordCount
        {
            get { return wordCount; }
            set { wordCount = value; OnPropertyChanged(nameof(WordCount)); }
        }

        // Шлях до файлу.
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        // Конструктор: завантажує документ зі шляху.
        public Document(string path)
        {
            // Потокобезпечне інкрементування для отримання унікального ID.
            this.Id = System.Threading.Interlocked.Increment(ref nextId);

            this.FilePath = path;
            this.Title = System.IO.Path.GetFileName(path);

            // Виклик методу для завантаження тексту та обчислення прев'ю.
            (this as IDocument).RefreshPreview();
        }

        // Явна реалізація інтерфейсу. Завантажує текст та обчислює метадані.
        void IDocument.RefreshPreview()
        {
            try
            {
                // 1. Зчитуємо весь текст із файлу в пам'ять.
                this.FullText = File.ReadAllText(this.FilePath);

                // 2. Рахуємо слова: розбиваємо текст за роздільниками і рахуємо непорожні частини.
                char[] separators = new char[] { ' ', '\r', '\n', '\t' };
                long count = this.FullText.Split(separators, StringSplitOptions.RemoveEmptyEntries).Length;

                // 3. Робимо Preview: беремо перші 150 символів.
                string previewText = this.FullText.Length > 150
                    ? this.FullText.Substring(0, 150) + "..."
                    : this.FullText;

                // Оновлюємо властивості (це викликає OnPropertyChanged через сеттери).
                this.WordCount = count;
                // Замінюємо переноси рядків у прев'ю на пробіли, щоб уникнути проблем з відображенням.
                this.Preview = previewText.Replace("\n", " ").Replace("\r", " ");
            }
            catch (Exception ex)
            {
                // Обробка помилок читання файлу (напр., "доступ заборонено").
                this.Preview = $"Error: {ex.Message}";
                this.WordCount = 0;
            }
        }
    }
}
