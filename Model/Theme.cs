using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Model
{
    // Enum Theme: Об'єкт даних (Model), який визначає можливі стани теми інтерфейсу.
    // Використовується для забезпечення типу та обмеження вибору (наприклад, у ViewModel).
    public enum Theme
    {
        // Світла тема.
        Light,

        // Темна тема.
        Dark
    }
}