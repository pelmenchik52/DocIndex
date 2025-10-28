using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    //буде singleton
    //відношення композиції ( документи належать корпусу)
    interface ICorpus
    {
        public ObservableCollection<IDocument> Documents { get; set; }

        public void AddDocument(IDocument document);
        public void RemoveDocument(IDocument document);
        public void RemoveAllDocuments();
        public void Clear();

        
    }
}
