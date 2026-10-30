using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using DocIndex.ViewModel;
using DocIndex.Model;

namespace DocIndex.Localization
{
    // Клас є Singleton-сервісом, що керує поточною мовою і сповіщає про її зміну.
    public class LocalizationManager : ModelBase
    {
        // Статичний доступ до єдиного екземпляра менеджера.
        public static LocalizationManager Instance { get; } = new LocalizationManager();

        // Ініціалізація ResourceManager, який зчитує вбудовані ресурсні файли (.resx).
        private readonly ResourceManager _resourceManager =
            new ResourceManager("DocIndex.Properties.Localizations.Resources", typeof(LocalizationManager).Assembly);

        private CultureInfo _currentCulture;

        // Властивість, яку ViewModel змінюватиме для перемикання мови.
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;

                    // Оновлюємо культуру потоку, щоб стандартні функції .NET (напр., string.Format) 
                    // та інші класи, що використовують CurrentUICulture, працювали правильно.
                    System.Threading.Thread.CurrentThread.CurrentUICulture = value;
                    System.Threading.Thread.CurrentThread.CurrentCulture = value;


                    // Виклик OnPropertyChanged з null або string.Empty
                    // сповіщає всі прив'язки до цього об'єкта (всі this[key]),
                    // що вони мають оновитися і отримати локалізований рядок новою мовою.
                    OnPropertyChanged(string.Empty);
                }
            }
        }   

        // Дозволяє XAML-біндінгу отримувати рядок за ключем (Binding Path=[key]).
        public string this[string key]
        {
            get
            {
                // Отримуємо рядок з .resx, використовуючи поточну обрану культуру.
                // Якщо рядок не знайдено, ResourceManager поверне null.
                var localizedString = _resourceManager.GetString(key, _currentCulture);

                // Повертаємо локалізований рядок, або ключ у дужках для діагностики.
                return localizedString ?? $"!{key}!";
            }
        }

        // Приватний конструктор, щоб забезпечити, що об'єкт створюється лише один раз (Singleton).
        private LocalizationManager()
        {
            // Встановлюємо початкову культуру (наприклад, українську) при старті програми.
            CurrentCulture = new CultureInfo("uk-UA");
        }
    }
}