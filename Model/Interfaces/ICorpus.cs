using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Інтерфейс ICorpus: Визначає контракт для центрального сховища документів (Corpus).
    // Реалізуючи цей інтерфейс, клас-сховище стає доступним для ViewModel.
    // Успадкування і відношення Композиції (документи є частиною корпусу) реалізуються в конкретному класі.
    interface ICorpus
    {

        // ObservableCollection: Властивість, що містить документи. ObservableCollection
        // дозволяє View (UI) автоматично оновлюватися при додаванні чи видаленні елементів.
        public ObservableCollection<IDocument> Documents { get; set; }

        // Додає новий документ до корпусу.
        public void AddDocument(IDocument document);

        // Видаляє конкретний документ із корпусу.
        public void RemoveDocument(IDocument document);

        // Повністю очищує корпус від усіх документів.
        public void RemoveAllDocuments();

        // Пошук і повернення документа за його унікальним ідентифікатором.
        public IDocument GetDocumentById(int docId);
    }
}
