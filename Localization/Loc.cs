using KURSOVA.Properties.Localizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KURSOVA.Localization
{
    // Статичний допоміжний клас для централізованого отримання локалізованих рядків
    public static class Loc
    {
        // Отримує локалізований рядок за ключем
        public static string GetString(string key)
        {
            // Отримуємо рядок із менеджера ресурсів, використовуючи поточну UI-культуру потоку.
            // CurrentUICulture визначає, який саме мовний ресурсний файл (.resx) буде використано.
            string localizedString = Resources.ResourceManager.GetString(
                key,
                Thread.CurrentThread.CurrentUICulture
            );

            // Повертаємо локалізований рядок. Якщо він порожній або null, повертаємо ключ
            // у форматі "!key!" для полегшення діагностики відсутніх рядків.
            return string.IsNullOrWhiteSpace(localizedString) ? $"!{key}!" : localizedString;
        }

        // Отримує локалізований рядок, а потім форматує його, підставляючи аргументи.
        public static string GetString(string key, params object[] args)
        {

            // Спочатку отримуємо базовий форматувальний рядок (напр. "Користувачів: {0}")
            string formatString = GetString(key);

            // Якщо аргументів немає, повертаємо лише базовий рядок, без форматування
            if (args == null || args.Length == 0)
            {
                return formatString;
            }

            try
            {
                // Форматуємо рядок, використовуючи поточну культуру для коректного форматування чисел, дат тощо, які є в 'args'
                return string.Format(Thread.CurrentThread.CurrentCulture, formatString, args);
            }
            catch (FormatException)
            {
                // Якщо форматування не вдалося, повертаємо неформатований рядок з попередженням
                return $"!FORMAT_ERR:{key}!";
            }
        }
    }
}
