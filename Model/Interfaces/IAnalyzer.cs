using KURSOVA.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    //буде strategy
    interface IAnalyzer
    {

        IEnumerable<KeywordItem> CalculateTfIdf(IEnumerable<IDocument> documents);
    }
}
