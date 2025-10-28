using KURSOVA.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    class Analyzer: IAnalyzer
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
             "і", "в", "з", "на", "та", "до", "не", "по", "для", "про", "він", "вона", // ... etc. ...
             "який", "яка", "яке", "які", "й", "цей", "ця", "ці", "того", "тому", "щоб", "1"
        };

        public IEnumerable<KeywordItem> CalculateTfIdf(IEnumerable<IDocument> documents)
        {
            if (documents == null || !documents.Any())
            {
                return Enumerable.Empty<KeywordItem>();
            }

            // N - загальна кількість документів у корпусі
            int totalDocCount = documents.Count();
            var globalWordFrequency = new Dictionary<string, int>();
            // n_t - кількість документів, де слово t зустрічається
            var docFrequency = new Dictionary<string, int>();

            // --- Step 1: Gather statistics ---
            foreach (var doc in documents)
            {
                var words = Regex.Split(doc.FullText.ToLower(), @"\W+")
                                 .Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 2 && !StopWords.Contains(w));
                var uniqueWordsInDoc = new HashSet<string>();
                foreach (var word in words)
                {
                    globalWordFrequency.TryAdd(word, 0); globalWordFrequency[word]++;
                    uniqueWordsInDoc.Add(word);
                }
                foreach (var uniqueWord in uniqueWordsInDoc)
                {
                    docFrequency.TryAdd(uniqueWord, 0); docFrequency[uniqueWord]++;
                }
            }

            // --- Step 2: Calculate TF-IDF ---
            var keywordList = new List<KeywordItem>();
            foreach (var (word, totalFreq) in globalWordFrequency)
            {
                if (!docFrequency.ContainsKey(word)) continue;

                // n_t for the current word
                int docsContaining = docFrequency[word];

                // --- MODIFIED IDF CALCULATION ---
                // Using the formula IDF = log(N / (1 + n_t))
                // Using Log10 for more manageable numbers
                double idf = Math.Log10((double)totalDocCount / (1 + docsContaining));
                // ---------------------------------

                // Score is still TF * IDF (using totalFreq as a corpus-wide TF approximation)
                double score = totalFreq * idf;

                keywordList.Add(new KeywordItem { Word = word, Frequency = totalFreq, Score = score });
            }

            // --- Step 3: Sort ---
            return keywordList.OrderByDescending(k => k.Score);
        }

    }
}
