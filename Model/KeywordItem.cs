using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model.Interfaces
{
    // Клас KeywordItem: Об'єкт даних (Model), що представляє слово
    // та його важливість (ключовість) у корпусі або кластері.
    public class KeywordItem
    {
        // Саме слово (термін).
        public string Word { get; set; }

        // Оцінка ключовості, розрахована за формулою ( TF * IDF).
        public double Score { get; set; }

        // Загальна кількість входжень слова (частота) у відповідному контексті (корпусі чи кластері).
        public int Frequency { get; set; }
    }
}