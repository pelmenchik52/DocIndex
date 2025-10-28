using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    public interface IDocument
    {
        //імя документу та вміст
        public string Title {  get; set; }
        public string Preview { get; set; }

        public string FullText { get; }

        //кількість слів та час імпорту документа
        public long WordCount { get; set; }
        public DateTime importDate {  get; set; }

        //шлях до документа
        public string FilePath { get; set; }
        
        void RefreshPreview();
    }
}
