using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Клас SimilarityResult: Об'єкт даних (Model), що зберігає результат 
    // порівняння одного документа з цільовим документом.
    public class SimilarityResult
    {
        // Унікальний ідентифікатор схожого документа.
        public int DocumentId { get; set; }

        // Заголовок (ім'я) схожого документа, для зручного відображення у View.
        public string DocumentTitle { get; set; }

        // Оцінка подібності (значення косинусної подібності, від 0.0 до 1.0).
        public double Score { get; set; }
    }
}