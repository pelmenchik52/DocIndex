using System;
using System.Windows.Data; // <-- Для Binding
using System.Windows.Markup;
using System.ComponentModel;
using KURSOVA.Localization;
using Microsoft.VisualBasic;
using System.Globalization;
using KURSOVA.Properties.Localizations;
using System.Threading;

namespace KURSOVA.Localization
{
    // Це реалізація вашого {loc:Loc KeyName}
    public class LocExtension : MarkupExtension
    {
        // Ключ, який ви передаєте (напр., "Menu_File")
        public string Key { get; set; }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Створюємо хитру прив'язку (Binding), 
            // яка буде реагувати на зміни мови

            var binding = new Binding
            {
                // Джерело - наш Singleton
                Source = LocalizationManager.Instance,

                // Ми не прив'язуємося до властивості,
                // тому що LocalizationManager сповіщає про зміну ВСІХ властивостей (null)
                Mode = BindingMode.OneWay,

                // Конвертер, який буде брати ключ і повертати рядок з .resx
                Converter = new LanguageConverter(Key)
            };

            // Повертаємо саму прив'язку
            return binding.ProvideValue(serviceProvider);
        }

        // Внутрішній конвертер, який робить всю роботу
        private class LanguageConverter : IValueConverter
        {
            private string _key;
            public LanguageConverter(string key)
            {
                _key = key;
            }

            // Цей метод буде викликатися КОЖЕН РАЗ, 
            // коли LocalizationManager подає сигнал про зміну мови
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                // Strings - це наш .resx файл (має бути public)
                // Ми просимо його дати нам рядок за ключем _key,
                // використовуючи ПОТОЧНУ культуру (яку ми встановили у ViewModel)
                return Resources.ResourceManager.GetString(_key, Thread.CurrentThread.CurrentUICulture);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}