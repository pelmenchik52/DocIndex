using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    // Клас FrequencyItem: Об'єкт даних (Model), що представляє слово та його частоту.
    // Використовується для зберігання результатів підрахунку слів.
    public class FrequencyItem
    {
        // Слово (термін), яке було знайдено.
        public string Word { get; set; }

        // Кількість разів, яку слово зустрічається (частота).
        public int Count { get; set; }
    }
}