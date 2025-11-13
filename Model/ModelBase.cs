using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KURSOVA.Model
{
    // ModelBase: Базовий клас для об'єктів Model, які повинні сповіщати UI про зміни.
    public abstract class ModelBase : INotifyPropertyChanged
    {
        // Подія, необхідна для INotifyPropertyChanged.
        public event PropertyChangedEventHandler? PropertyChanged;

        // Метод для виклику події PropertyChanged.
        // [CallerMemberName] дозволяє викликати метод без явного вказування імені властивості.
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}