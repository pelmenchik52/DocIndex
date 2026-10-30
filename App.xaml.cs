using DocIndex.Model;
using DocIndex.ViewModel;
using System.Windows;
using System.Linq;
using DocIndex.Services; 
using System.Globalization;
using DocIndex.Localization;

namespace DocIndex
{
    // Точка входу, відповідає за ініціалізацію інтерфейсу, створення сервісів
    // та застосування початкових налаштувань.
    public partial class App : Application
    {
        // Створюємо та зберігаємо Singleton-сервіси
        private readonly IDialogService _dialogService = new DialogService();
        private readonly IThemeService _themeService = new ThemeService();

        private SettingViewModel _settingsVM;
        private MainViewModel _mainVM;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Створення SettingViewModel для завантаження (не відображення!) налаштувань.
            // Передаємо сервіси та null Action, оскільки вікно тут не відкривається.
            _settingsVM = new SettingViewModel(_dialogService, _themeService, null);

            // 2. Застосовуємо завантажені налаштування через чисті сервіси.
            ApplyLoadedSettings();

            // 3. Ініціалізація головної ViewModel
            _mainVM = new MainViewModel(_dialogService, _themeService);

            // Створення MainWindow і встановлення контексту даних
            var mainWindow = new MainWindow
            {
                DataContext = _mainVM
            };
            mainWindow.Show();
        }

        // Чистий метод для застосування налаштувань через сервіси.
        private void ApplyLoadedSettings()
        {
            // 1. Застосовуємо тему через IThemeService.
            _themeService.ApplyTheme(_settingsVM.SelectedTheme);

            // 2. Застосовуємо мову через LocalizationManager.
            if (_settingsVM.SelectedLanguage != null)
            {
                var culture = new CultureInfo(_settingsVM.SelectedLanguage.CultureName);
                LocalizationManager.Instance.CurrentCulture = culture;
            }
        }
    }
}