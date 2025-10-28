using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model
{

    public class Corpus
    {
        public static Corpus Instance { get; } = new Corpus();
        public ObservableCollection<IDocument> Documents { get; set; }
 


        //конструктор
        private Corpus() { 
            Documents = new ObservableCollection<IDocument>();
        }

        //додати документ
        public void AddDocument(IDocument document) {
            Documents.Add(document);

        }

        //видалити документ
        public void RemoveDocument(IDocument document)
        {
            Documents.Remove(document);
        }

        //видалити всі документи
        public void RemoveAllDocuments()
        {
            Documents?.Clear();
        }
    }
}
