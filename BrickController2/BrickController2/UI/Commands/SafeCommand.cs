using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.Commands
{
    public class SafeCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;

        private bool _allowExecute = true;

        public SafeCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = o => { execute?.Invoke(); return Task.FromResult(true); };
            _canExecute = o => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Action<object> execute, Func<bool> canExecute = null)
        {
            _execute = o => { execute?.Invoke(o); return Task.FromResult(true); };
            _canExecute = o => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = async o => await execute?.Invoke();
            _canExecute = o => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (o => true);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter) && _allowExecute;
        }

        public async void Execute(object parameter)
        {
            _allowExecute = false;
            RaiseCanExecuteChanged();

            try
            {
                await _execute?.Invoke(parameter);
            }
            finally
            {
                _allowExecute = true;
                RaiseCanExecuteChanged();
            }
        }

        private void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SafeCommand<TExecute> : ICommand
    {
        private readonly Func<TExecute, Task> _execute;
        private readonly Predicate<object> _canExecute;

        private bool _allowExecute = true;

        public SafeCommand(Action<TExecute> execute, Func<bool> canExecute = null)
        {
            _execute = o => { execute?.Invoke(o); return Task.FromResult(true); };
            _canExecute = o => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Func<TExecute, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = async o => await execute?.Invoke(o);
            _canExecute = o => canExecute?.Invoke(o) ?? true;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter) && _allowExecute;
        }

        public async void Execute(object parameter)
        {
            _allowExecute = false;
            RaiseCanExecuteChanged();

            try
            {
                await _execute?.Invoke((TExecute)parameter);
            }
            finally
            {
                _allowExecute = true;
                RaiseCanExecuteChanged();
            }
        }

        private void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
