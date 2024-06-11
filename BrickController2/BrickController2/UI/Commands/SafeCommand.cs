using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.Commands
{
    public class SafeCommand : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Predicate<object?> _canExecute;

        private bool _allowExecute = true;

        public SafeCommand(Action execute, Func<bool>? canExecute = null)
        {
            _executeAsync = _ => { execute?.Invoke(); return Task.FromResult(true); };
            _canExecute = _ => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Action<object?> execute, Func<bool>? canExecute = null)
        {
            _executeAsync = o => { execute?.Invoke(o); return Task.FromResult(true); };
            _canExecute = _ => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _executeAsync = async _ => await execute.Invoke();
            _canExecute = _ => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
        {
            _executeAsync = execute;
            _canExecute = canExecute ?? (_ => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _allowExecute && _canExecute(parameter);
        }

        public async void Execute(object? parameter)
        {
            _allowExecute = false;
            RaiseCanExecuteChanged();

            try
            {
                await (_executeAsync?.Invoke(parameter) ?? Task.FromResult(true));
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
        private readonly Func<TExecute, Task> _executeAsync;
        private readonly Predicate<object?> _canExecute;

        private bool _allowExecute = true;

        public SafeCommand(Action<TExecute> execute, Func<bool>? canExecute = null)
        {
            _executeAsync = o => { execute?.Invoke(o); return Task.FromResult(true); };
            _canExecute = _ => canExecute?.Invoke() ?? true;
        }

        public SafeCommand(Func<TExecute, Task> executeAsync, Predicate<object?>? canExecute = null)
        {
            _executeAsync = async o => await executeAsync.Invoke(o);
            _canExecute = o => canExecute?.Invoke(o) ?? true;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _allowExecute && _canExecute(parameter);
        }

        public async void Execute(object? parameter)
        {
            _allowExecute = false;
            RaiseCanExecuteChanged();

            try
            {
                await _executeAsync.Invoke((TExecute)parameter!);
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
