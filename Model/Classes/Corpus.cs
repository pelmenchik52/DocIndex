using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Клас Corpus: центральне сховище для всіх документів. 
    public class Corpus
    {
        // Singleton: забезпечує єдиний екземпляр сховища для всього додатка.
        public static Corpus Instance { get; } = new Corpus();

        // ObservableCollection: використовується для автоматичного оновлення UI (View),
        // коли документи додаються, видаляються або переміщуються.
        public ObservableCollection<IDocument> Documents { get; set; }

        // Приватний конструктор, щоб запобігти створенню нових екземплярів (Singleton).
        private Corpus()
        {
            Documents = new ObservableCollection<IDocument>();
        }

        // Додає документ до корпусу. UI автоматично оновлюється.
        public void AddDocument(IDocument document)
        {
            Documents.Add(document);
        }

        // Видаляє документ з корпусу. UI автоматично оновлюється.
        public void RemoveDocument(IDocument document)
        {
            Documents.Remove(document);
        }

        // Видаляє всі документи з корпусу.
        public void RemoveAllDocuments()
        {
            // УВАГА: Виклик Dispatcher створює пряму залежність Model від WPF.
            // Його можна видалити, якщо ви впевнені, що метод не викликається з фонового потоку.
            // Якщо він викликається з фону, логіка Dispatcher має бути у ViewModel.
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Метод Clear() скидає колекцію, генеруючи подію для UI.
                this.Documents.Clear();
            });
        }

        // Пошук документа за його унікальним ідентифікатором.
        public IDocument GetDocumentById(int docId)
        {
            // Використовуємо LINQ: повертає перший документ, що відповідає умові, або null.
            return Documents.FirstOrDefault(doc => doc.Id == docId);
        }
    }
}