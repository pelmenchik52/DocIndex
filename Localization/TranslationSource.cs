using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace KURSOVA.Localization
{
    public class TranslationSource : INotifyPropertyChanged
    {
        public static TranslationSource Instance { get; } = new TranslationSource();

        // 2. Власне, самі ресурси
        private readonly ResourceManager _resourceManager =
            new ResourceManager("KURSOVA.Properties.Localizations.Resources", typeof(TranslationSource).Assembly);

        // 3. Подія, якої вимагає INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private CultureInfo _currentCulture;
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;

                    // *** КРИТИЧНИЙ РЯДОК! ***
                    // Це повідомляє WPF, що ВСІ прив'язки до цього об'єкта
                    // (тобто всі індекси this[key]) застаріли і їх треба оновити.
                    OnPropertyChanged(string.Empty); // або OnPropertyChanged(null);
                }
            }
        }

        // 4. Індексатор, який LocExtension буде використовувати
        public string this[string key]
        {
            get
            {
                // Отримуємо рядок з .resx, використовуючи поточну обрану культуру
                return _resourceManager.GetString(key, _currentCulture);
            }
        }

        // 5. Метод-помічник для виклику події
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Конструктор (встановлює мову за замовчуванням)
        private TranslationSource()
        {
            // Встановлюємо початкову культуру
            CurrentCulture = new CultureInfo("uk-UA"); // Або "uk-UA"
        }
    }
}
