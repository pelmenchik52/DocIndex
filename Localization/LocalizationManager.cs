using System;
using System.ComponentModel; 
using System.Runtime.CompilerServices;

namespace KURSOVA.Localization
{

    public sealed class LocalizationManager : INotifyPropertyChanged
    {

        private static readonly LocalizationManager _instance = new LocalizationManager();
        public static LocalizationManager Instance => _instance;

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseLanguageChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}