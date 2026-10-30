using DocIndex.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocIndex.Model.Interfaces
{
    // Інтерфейс IKMeansCluster: Визначає контракт для служби, яка виконує
    // алгоритм кластеризації K-Means на документах.
    public interface IKMeansCluster
    {
        // Метод Cluster: Запускає алгоритм кластеризації.
        // k: Кількість кластерів, які потрібно сформувати (напр., 3, 5).
        // maxIterations: Максимальна кількість ітерацій алгоритму для досягнення збіжності.
        // Повертає List<Cluster>: список фінальних кластерів, які містять центроїди та ID призначених документів.
        List<Cluster> Cluster(int k, int maxIterations);

    }
}
