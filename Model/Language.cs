using DocIndex.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocIndex.Model
{
    // Об'єкт, що представляє доступну мову.
    public class Language: ModelBase
    {
            public string DisplayName { get; set; } // Ім'я для відображення у ComboBox (напр. "Українська")
            public string CultureName { get; set; } // Код культури (напр. "uk-UA")

            // Використовується ComboBox-ом для відображення тексту.
            public override string ToString()
            {
                return DisplayName;
            }
        
    }
}
