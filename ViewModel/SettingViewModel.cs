using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.IO;
using System.Text.Json;
using KURSOVA.Model;
using KURSOVA.Localization;
using KURSOVA.Commands;
using KURSOVA.Services;

namespace KURSOVA.ViewModel
{
    // ViewModel для керування налаштуваннями програми (тема та мова).
    public class SettingViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IThemeService _themeService;
        private Action _closeAction;

        //  Властивості Налаштувань 
        private Theme _selectedTheme;
        public Theme SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                OnPropertyChanged(nameof(SelectedTheme));
            }
        }

        // Мови
        public List<Language> AvailableLanguages { get; set; }

        private Language _selectedLanguage;
        // Вибрана мова
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }

        //  Команди 
        public ICommand ApplyCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        // Конструктор
        public SettingViewModel(IDialogService dialogService, IThemeService themeService, Action closeAction)
        {
            _dialogService = dialogService;
            _themeService = themeService;
            _closeAction = closeAction;

            ApplyCommand = new RelayCommand(OnApply, CanAlwaysExecute);
            CancelCommand = new RelayCommand(OnCancel, CanAlwaysExecute);

            AvailableLanguages = new List<Language>
            {
                new Language { DisplayName = "English", CultureName = "en-US" },
                new Language { DisplayName = "Українська", CultureName = "uk-UA" }
            };

            LoadSettings();
        }

        // Логіка iніціалізації 
        private void LoadSettings()
        {
            var currentCulture = LocalizationManager.Instance.CurrentCulture;

            _selectedLanguage = AvailableLanguages.FirstOrDefault(
                lang => lang.CultureName == currentCulture.Name
            ) ?? AvailableLanguages.FirstOrDefault(l => l.CultureName == "uk-UA");

            _selectedTheme = Theme.Light;

            OnPropertyChanged(nameof(SelectedLanguage));
            OnPropertyChanged(nameof(SelectedTheme));
        }

        // Логіка команд
        private void OnApply(object parameter)
        {
            // 1. Застосовуємо тему через IThemeService.
            _themeService.ApplyTheme(SelectedTheme);

            // 2. Змінюємо культуру.
            if (SelectedLanguage != null)
            {
                var culture = new CultureInfo(SelectedLanguage.CultureName);
                LocalizationManager.Instance.CurrentCulture = culture;
            }

            // 3. Зберігаємо зміни.
            SaveSettingsToFile();

            // 4. Закриваємо вікно через Action.
            _closeAction?.Invoke();
        }

        private void OnCancel(object parameter)
        {
            // 1. Якщо потрібно, завантажуємо старі налаштування (якщо не було Apply).
            // 2. Закриваємо вікно через Action.
            _closeAction?.Invoke();
        }

        private bool CanAlwaysExecute(object obj)
        {
            return true;
        }

        // Логіка збереження
        private const string SettingsFilePath = "settings.json";

        public void SaveSettingsToFile()
        {
            try
            {
                var settings = new Dictionary<string, string>
               {
                 { "Theme", SelectedTheme.ToString() },
                 { "Language", SelectedLanguage?.CultureName ?? "uk-UA" }
               };

                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                // Використовуємо _dialogService для логування/повідомлення про помилки I/O
                _dialogService.ShowMessage($"Error during saving file: {ex.Message}", "Error");
            }
        }
    }
}