using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model.Interfaces
{
    public class KeywordItem
    {
        public string Word { get; set; }    // Слово
        public double Score { get; set; }   // Його TF-IDF бал
        public int Frequency { get; set; } // Загальна кількість у кластері
    }
}
