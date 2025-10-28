using System;
using System.Globalization;
using System.Windows.Data;

// ВАЖЛИВО: Замініть 'KURSOVA.Model' на простір імен, 
// де знаходиться ваш enum 'Theme'. 
// Якщо він у моделі, то цей 'using' правильний.
using KURSOVA.Model;

namespace KURSOVA.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        // Цей метод конвертує ваш enum (Theme.Light/Theme.Dark) у bool
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Отримуємо рядок з enum (напр., "Light")
            string enumValue = value.ToString();
            // Отримуємо рядок з параметра кнопки (напр., "Light")
            string targetValue = parameter.ToString();

            // Якщо вони збігаються, кнопка має бути відмічена (true)
            return enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        // Цей метод конвертує bool (кнопка натиснута) назад у ваш enum
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                // Припускаємо, що ваш enum називається 'Theme'
                // і знаходиться у просторі імен, що підключений через 'using'
                return Enum.Parse(typeof(Theme), parameter.ToString());
            }
            return Binding.DoNothing;
        }
    }
}