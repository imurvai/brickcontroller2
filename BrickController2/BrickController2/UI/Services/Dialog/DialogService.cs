using BrickController2.PlatformServices.GameController;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Dialog
{
    public class DialogService : IDialogService, IDialogServerHost
    {
        private readonly IGameControllerService _gameControllerService;

        private IDialogServer? _dialogServer;

        public bool IsDialogOpen => _dialogServer?.IsDialogOpen ?? false;

        public DialogService(IGameControllerService gameControllerService)
        {
            _gameControllerService = gameControllerService;
        }

        public Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token)
        {
            return _dialogServer?.ShowMessageBoxAsync(title, message, buttonText, token) ?? Task.CompletedTask;
        }

        public Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token)
        {
            return _dialogServer?.ShowQuestionDialogAsync(title, message, positiveButtonText, negativeButtonText, token) ?? Task.FromResult(false);
        }

        public Task<InputDialogResult> ShowInputDialogAsync(string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, KeyboardType keyboardType, Predicate<string> valuePredicate, CancellationToken token)
        {
            return _dialogServer?.ShowInputDialogAsync(initialValue, placeHolder, positiveButtonText, negativeButtonText, keyboardType, valuePredicate, token) ?? Task.FromResult(new InputDialogResult(false, string.Empty));
        }

        public Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items, string title, string cancelButtonText, CancellationToken token) where T : notnull
        {
            return _dialogServer?.ShowSelectionDialogAsync(items, title, cancelButtonText, token) ?? Task.FromResult(new SelectionDialogResult<T>(false, default!));
        }

        public Task<ProgressDialogResult> ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title, string? message, string? cancelButtonText, CancellationToken token)
        {
            return _dialogServer?.ShowProgressDialogAsync(isDeterministic, action, title, message, cancelButtonText, token) ?? Task.FromResult(new ProgressDialogResult(false));
        }

        public Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token)
        {
            if (_dialogServer is not null)
            {
                _dialogServer.GameControllerService = _gameControllerService;
            }

            return _dialogServer?.ShowGameControllerEventDialogAsync(title, message, cancelButtonText, token) ?? Task.FromResult(new GameControllerEventDialogResult(false, GameControllerEventType.Axis, string.Empty));
        }

        public void RegisterDialogServer(IDialogServer dialogServer)
        {
            _dialogServer = dialogServer;
        }

        public void UnregisterDialogServer(IDialogServer dialogServer)
        {
            // reset only in case previously set server matches
            if (_dialogServer == dialogServer)
            {
                _dialogServer = null;
            }
        }
    }
}
