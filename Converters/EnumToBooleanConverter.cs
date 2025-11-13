using System;
using System.Globalization;
using System.Windows.Data;
using KURSOVA.Model;

namespace KURSOVA.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        //Цей метод конвертує enum (Theme.Light/Theme.Dark) у bool
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            //Отримуємо рядок з enum (напр., "Light")
            string enumValue = value.ToString();
            //Отримуємо рядок з параметра кнопки (напр., "Light")
            string targetValue = parameter.ToString();

            //Якщо вони збігаються, кнопка має бути відмічена (true)
            return enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }

        //Цей метод конвертує bool (кнопка натиснута) назад у enum
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                // Припускаємо, що enum називається 'Theme' і знаходиться у просторі імен, що підключений через 'using'
                return Enum.Parse(typeof(Theme), parameter.ToString());
            }
            return Binding.DoNothing;
        }
    }
}