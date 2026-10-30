using DocIndex.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Інтерфейс IAnalyzer: Визначає контракт для класів, що виконують
    // аналіз корпусу (бізнес-логіку), наприклад, обчислення TF-IDF, 
    // статистичний аналіз, та пошук подібності документів.
    // Це частина патерну Strategy. В подальшому в Analyzer ми можемо додати
    // інше визначення методу FindSimilarDocuments (наприклад, LSI, Word2Vec тощо), але поки у нас лише косинусна подібність.

    
    public interface IAnalyzer
    {
        // Повертає список документів, ранжованих за подібністю до введеного запиту.
        List<SimilarityResult> SearchByQuery(string query, double similarityThreshold);
        // Повертає список ключових слів у всьому корпусі, відсортований за їхньою оцінкою (Score).
        IEnumerable<KeywordItem> GetCorpusKeywords();

        // Повертає загальну кількість унікальних слів (розмір словника) у корпусі.
        int GetUniqueWordCount();

        // Повертає загальну кількість усіх оброблених слів у корпусі (включаючи повтори).
        int GetTotalProcessedWordCount();

        // Виконує пошук документів, схожих на вказаний цільовий документ (targetDocument),
        // використовуючи метрику подібності (косинусну подібність).
        // Повертає список результатів, які перевищують заданий поріг (similarityThreshold).
        List<SimilarityResult> FindSimilarDocuments(IDocument targetDocument, double similarityThreshold);
    }
}