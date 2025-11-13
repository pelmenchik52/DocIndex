using KURSOVA.Model; // Для Theme
using KURSOVA.Services; // Для IThemeService
using System;
using System.Linq;
using System.Windows; // Для Application, ResourceDictionary, Uri

namespace KURSOVA.Services
{
    // ThemeService: Конкретна реалізація служби зміни теми.
    // Тут міститься логіка, яка взаємодіє з ResourceDictionary WPF.
    public class ThemeService : IThemeService
    {
        // Примітка: Ця логіка виконується тільки в UI-потоці.
        public void ApplyTheme(Theme theme)
        {
            var appDictionaries = Application.Current.Resources.MergedDictionaries;

            // Визначаємо URI нової теми
            string themeFileName = (theme == Theme.Light) ? "LightTheme.xaml" : "DarkTheme.xaml";
            string themeUri = $"/Themes/{themeFileName}";
            var newThemeUri = new Uri(themeUri, UriKind.Relative);

            // 1. Шукаємо стару тему в об'єднаних словниках ресурсів
            var oldTheme = appDictionaries
                .FirstOrDefault(d => d.Source != null && (
                    d.Source.OriginalString.EndsWith("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) ||
                    d.Source.OriginalString.EndsWith("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase)
                ));

            // 2. Якщо знайдено стару тему
            if (oldTheme != null)
            {
                // Якщо намагаємося застосувати ту саму тему, нічого не робимо
                if (oldTheme.Source.OriginalString.EndsWith(themeUri, StringComparison.OrdinalIgnoreCase))
                    return;

                // Видаляємо стару тему
                appDictionaries.Remove(oldTheme);
            }

            // 3. Створюємо та вставляємо нову тему
            var newTheme = new ResourceDictionary { Source = newThemeUri };

            // Вставляємо нову тему на початок, щоб її ресурси мали пріоритет.
            appDictionaries.Insert(0, newTheme);
        }
    }
}