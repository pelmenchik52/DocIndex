using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Інтерфейс IIndexer: Визначає контракт для служби, яка створює та зберігає
    // інвертований індекс корпусу та обчислює метрики TF-IDF.
    public interface IIndexer
    {
        // Ініціює процес токенізації, нормалізації та обчислення TF-IDF для всього корпусу.
        void IndexCorpus(IEnumerable<IDocument> documents);

        // Повертає TF-IDF вектор (ваги) для всіх слів у конкретному документі.
        // Ключ: слово, Значення: TF-IDF оцінка.
        Dictionary<string, double> GetDocumentVector(int docId);

        // Повертає словник оберненої частоти документа (IDF) для всього корпусу.
        // Ключ: слово, Значення: IDF оцінка.
        Dictionary<string, double> GetIdfDictionary();

        // Повертає загальну кількість входжень кожного слова у всьому корпусі.
        // Ключ: слово, Значення: Загальна частота.
        Dictionary<string, int> GetGlobalWordFrequencies();

        // Повертає повний набір усіх унікальних слів, оброблених у корпусі.
        HashSet<string> GetVocabulary();

        // Повертає словник, що містить TF-IDF вектори для ВСІХ документів у корпусі.
        // Ключ: ID документа, Значення: TF-IDF вектор цього документа.
        Dictionary<int, Dictionary<string, double>> GetAllDocumentVectors();

        (Dictionary<string, int> wordCounts, int totalWords) Preprocess(string text);

        // Очищує всі внутрішні структури даних індексу (словники, вектори).
        void ClearIndex();
    }
}