using System.Windows.Input;

namespace KURSOVA.Commands
{

    //Клас RelayCommand реалізує інтерфейс ICommand для створення команд у WPF.
    public class RelayCommand : ICommand
    {

        //Поле для збереження дії виконання команди.
        private Action<object> _Execute { get; set; }

        //Поле для зберігання умови чи можна виконати команду
        private Predicate<object> _CanExecute { get; set; }

        // Подія CanExecuteChanged інтерфейсу ICommand. 
        // Вона сповіщає WPF про можливу зміну стану CanExecute.
        public event EventHandler? CanExecuteChanged
        {
            // Додаємо обробник до статичної події RequerySuggested менеджера команд,
            // який викликається, коли WPF вважає, що слід перевірити CanExecute всіх команд.
            add { CommandManager.RequerySuggested += value; }
            // Видаляємо обробник з події RequerySuggested.
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Конструктор класу RelayCommand.
        // Приймає два делегати: для дії виконання та для перевірки можливості виконання.
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            // Ініціалізуємо поле виконання команди.
            _Execute = execute;
            // Ініціалізуємо поле перевірки можливості виконання команди.
            _CanExecute = canExecute;
        }

        // Метод інтерфейсу ICommand. Викликається WPF для визначення, 
        // чи доступна команда (активна чи неактивна).
        public bool CanExecute(object? parameter)
        {
            // Викликаємо внутрішній делегат умови виконання
            return _CanExecute(parameter);
        }

        // Метод інтерфейсу ICommand. Викликається WPF, коли команда виконується.
        public void Execute(object? parameter)
        {
            _Execute(parameter);
        }

        // Публічний метод для примусового виклику перевірки CanExecute 
        // всіх підключених до CommandManager команд.
        public void RaiseCanExecuteChanged()
        {
            // Запускає подію RequerySuggested, змушуючи WPF перевірити CanExecute 
            // для всіх команд, які на неї підписані.
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
