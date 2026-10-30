using System;
using System.Collections.Generic;
namespace DocIndex.Model
{
    // Інтерфейс IDocument: Визначає контракт для основного об'єкта даних (Model).
    // Він представляє документ у корпусі з його метаданими та вмістом.
    public interface IDocument
    {
        // Унікальний ідентифікатор документа. 
        // 'init' дозволяє встановити значення лише в конструкторі, забезпечуючи незмінність.
        public int Id { get; init; }

        // Заголовок (ім'я) документа. Доступний для читання і запису.
        public string Title { get; set; }

        // Короткий попередній перегляд вмісту для відображення у списку. Доступний для читання і запису.
        public string Preview { get; set; }

        // Повний текстовий вміст документа. Доступний лише для читання ззовні.
        public string FullText { get; }

        // Кількість оброблених слів. Доступна для читання і запису.
        public long WordCount { get; set; }

        // Повний шлях до файлу на диску.
        public string FilePath { get; set; }

        // Метод, що ініціює повторне зчитування вмісту файлу, 
        // обчислення WordCount і оновлення Preview. (Бізнес-логіка Model).
        void RefreshPreview();
    }
}