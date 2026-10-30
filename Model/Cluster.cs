using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Клас Cluster: Об'єкт даних (Model), який зберігає фінальний результат кластеризації K-Means.
    public class Cluster
    {
        // Унікальний ідентифікатор кластера (напр., 0, 1, 2...).
        public int ClusterId { get; set; }

        // Центроїд кластера: вектор, що представляє "центр мас" кластера.
        // Ключ: слово (термін), Значення: TF-IDF оцінка.
        public Dictionary<string, double> Centroid { get; set; }

        // Список ідентифікаторів документів (DocId), які були призначені цьому кластеру.
        public List<int> DocumentIds { get; set; }

        // Конструктор: ініціалізує кластер з ID та початковим центроїдом.
        public Cluster(int id, Dictionary<string, double> centroid)
        {
            ClusterId = id;
            Centroid = centroid;
            // Ініціалізуємо список ID документів як порожній.
            DocumentIds = new List<int>();
        }
    }
}