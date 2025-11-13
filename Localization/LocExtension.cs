using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using KURSOVA.Properties.Localizations;

namespace KURSOVA.Localization
{
    //Розширення для XAML, яке дозволяє локалізувати рядки на основі ключа ({loc:Loc MyStringKey}).
    public class LocExtension : MarkupExtension
    {

        // Ключ ресурсу, який передається у XAML (напр., "ButtonTitle")
        public string Key { get; set; }

        // Констуктор, щоб у XAML ми могли створити розширення розмітки таким чином: {loc:Loc Key=New_Button}
        public LocExtension() { }

        // Конструктор для коротшого створення розширення розмітки: {loc:Loc New_Button}
        public LocExtension(string key)
        {
            Key = key;
        }

        // Головний метод, який повертає об'єкт Binding для динамічного оновлення тексту.
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Створюємо прив'язку до LocalizationManager
            var binding = new Binding
            {
                // Source: Використовуємо Singleton-екземпляр менеджера локалізації як джерело даних.
                Source = LocalizationManager.Instance,
                // Прив'язуємось до CurrentCulture, щоб отримувати оновлення, будь-яка зміна цієї властивості змушує Convert спрацьовувати.
                Path = new System.Windows.PropertyPath(nameof(LocalizationManager.CurrentCulture)),

                Mode = BindingMode.OneWay,

                // Converter: Спеціальний конвертер, який використовує наш Key для витягування рядка.
                Converter = new LanguageConverter(Key)
            };

            // Повертаємо Binding, який WPF розміщує у властивість UI-елемента.
            return binding.ProvideValue(serviceProvider);
        }

        // Допоміжний статичний метод для отримання локалізованого значення безпосередньо в коді C#.
        public static string GetLocalizedValue(string key)
        {
            CultureInfo currentCulture = LocalizationManager.Instance.CurrentCulture;

            var result = Resources.ResourceManager.GetString(key, currentCulture);

            // Якщо рядок не знайдено, повертаємо ключ у квадратних дужках для діагностики.
            return result ?? $"[{key}]";
        }

        // Вкладений приватний конвертер, який виконує фактичне витягування ресурсу.
        // Він викликається щоразу, коли CurrentCulture змінюється.
        private class LanguageConverter : IValueConverter
        {

            // Ключ, отриманий в XAML (тут він є лише для читання і змінити його не можна)
            private readonly string _key;

            // Конструктор, який присвоює полю _key ключ, витягнутий з файлу ресурсів
            public LanguageConverter(string key)
            {
                _key = key;
            }

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                // Отримуємо поточну культуру
                // value тут - це LocalizationManager.CurrentCulture
                var currentCulture = value as System.Globalization.CultureInfo
                                     // Використовуємо запасний варіант на випадок, якщо value null
                                     ?? LocalizationManager.Instance.CurrentCulture;

                // Отримуємо рядок з ResourceManager, використовуючи збережений ключ та культуру
                var result = Resources.ResourceManager.GetString(_key, currentCulture);

                // Якщо рядок не знайдено, повертаємо ключ
                return result ?? $"[{_key}]";
            }

            // Метод існує лише для того, якщо хтось спробує використовувати двонаправлену прив'язку
            // (це впринципі не можливо, цей метод існує на всяк випадок)
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}