using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;         // <-- Потрібно для ICommand
using KURSOVA.Model;
using KURSOVA.Localization;
using KURSOVA.Commands;           

namespace KURSOVA.ViewModel
{
    public class Language
    {
        public string DisplayName { get; set; }
        public string CultureName { get; set; }
    }

    class SettingViewModel : INotifyPropertyChanged
    {

        private Theme _originalTheme;
        private Language _originalLanguage;

 
        private Theme _selectedTheme;
        public Theme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                OnPropertyChanged();
               
            }
        }

        public List<Language> AvailableLanguages { get; set; }

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
               
            }
        }


        public ICommand ApplyCommand { get; set; }
        public ICommand CancelCommand { get; set; }



        public SettingViewModel()
        {
            ApplyCommand = new RelayCommand(OnApply, CanAlwaysExecute);
            CancelCommand = new RelayCommand(OnCancel, CanAlwaysExecute);

            AvailableLanguages = new List<Language>
            {
                new Language { DisplayName = "English", CultureName = "en-US" },
                new Language { DisplayName = "Українська", CultureName = "uk-UA" }
            };

            LoadCurrentSettings();
        }

        private void OnApply(object parameter)
        {
            ApplyLanguage(SelectedLanguage);
            ApplyTheme(SelectedTheme);

            SaveCurrentSettings();

           // CloseWindow(parameter);
        }

        // Метод для кнопки "Скасувати"
        private void OnCancel(object parameter)
        {
            // 1. Повертаємо тему до тої, яка була при відкритті
            if (_selectedTheme != _originalTheme)
            {
                ApplyTheme(_originalTheme);
            }
            // (Мову повертати не треба, бо ми її не застосовували)

            //CloseWindow(parameter);
        }



        private void LoadCurrentSettings()
        {
            // Наприклад, завантажуємо з налаштувань програми
            // (Переконайтеся, що ви створили 'Theme' і 'Language' у Properties.Settings.Default)
            // string savedLang = Properties.Settings.Default.Language;
            // string savedTheme = Properties.Settings.Default.Theme;

            // --- Завантаження Мови ---
            // (Поки що використовуємо поточну культуру, як у вашому коді)
            _selectedLanguage = AvailableLanguages.FirstOrDefault(
                lang => lang.CultureName == Thread.CurrentThread.CurrentUICulture.Name
            ) ?? AvailableLanguages[1]; // Українська за замовчуванням

            // --- Завантаження Теми ---
            // (Поки що Світла за замовчуванням, але краще завантажувати збережену)
            _selectedTheme = Theme.Light; // <-- Замініть на завантаження збереженої теми

            // Зберігаємо оригінальні значення для "Скасувати"
            _originalLanguage = _selectedLanguage;
            _originalTheme = _selectedTheme;

            // Оновлюємо UI, щоб показати завантажені налаштування
            OnPropertyChanged(nameof(SelectedLanguage));
            OnPropertyChanged(nameof(SelectedTheme));
        }

        private void SaveCurrentSettings()
        {
            // Зберігаємо у налаштування програми
            // Properties.Settings.Default.Language = SelectedLanguage.CultureName;
            // Properties.Settings.Default.Theme = SelectedTheme.ToString();
            // Properties.Settings.Default.Save();
        }


        private void ApplyLanguage(Language lang)
        {
            if (lang == null || lang.CultureName == Thread.CurrentThread.CurrentUICulture.Name)
                return;

            var culture = new CultureInfo(lang.CultureName);
            Thread.CurrentThread.CurrentUICulture = culture;
            LocalizationManager.Instance.RaiseLanguageChanged();
        }

        private void ApplyTheme(Theme theme)
        {
            var appDictionaries = Application.Current.Resources.MergedDictionaries;
            string themeUri = (theme == Theme.Light) ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";

            var oldTheme = appDictionaries.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("LightTheme.xaml") || d.Source.OriginalString.Contains("DarkTheme.xaml")));
            if (oldTheme != null)
            {
                // Якщо стара тема - це вже та, яку ми хочемо, нічого не робимо
                if (oldTheme.Source.OriginalString.Contains(themeUri))
                    return;
                appDictionaries.Remove(oldTheme);
            }

            var newTheme = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };
            appDictionaries.Insert(0, newTheme);
        }



        private bool CanAlwaysExecute(object obj)
        {
            return true; 
        }

        private void CloseWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}