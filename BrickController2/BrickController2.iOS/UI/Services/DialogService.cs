using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.HardwareServices;
using BrickController2.UI.Services;
using UIKit;

namespace BrickController2.iOS.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly IGameControllerService _gameControllerService;

        public DialogService(IGameControllerService gameControllerService)
        {
            _gameControllerService = gameControllerService;
        }

        public Task<IInputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string positiveButtonText, string negativeButtonText)
        {
            var completionSource = new TaskCompletionSource<IInputDialogResult>();
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            alert.AddTextField(textField => textField.Text = initialValue);

            alert.AddAction(UIAlertAction.Create(positiveButtonText ?? "Ok", UIAlertActionStyle.Default, action =>
            {
                completionSource.SetResult(new InputDialogResult { IsPositive = true, Result = alert.TextFields.First().Text });
            }));

            alert.AddAction(UIAlertAction.Create(negativeButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                completionSource.SetResult(new InputDialogResult { IsPositive = false, Result = alert.TextFields.First().Text });
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            return completionSource.Task;
        }

        public IProgress ShowProgressDialogAsync(string title, string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue)
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            if (tokenSource != null)
            {
                alert.AddAction(UIAlertAction.Create(cancelButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
                {
                    tokenSource.Cancel();
                }));
            }

            // TODO: add progress here
            UIProgressView progressView = null;

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            return new ProgressImpl(alert, progressView, minValue, maxValue);
        }

        public Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText)
        {
            var completionSource = new TaskCompletionSource<IGameControllerEventDialogResult>();
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            alert.AddAction(UIAlertAction.Create(cancelButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                completionSource.SetResult(new GameControllerDialogResult { IsOk = false, EventType = GameControllerEventType.Button, EventCode = null });
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            return completionSource.Task;

            async void GameControllerEventHandler(object sender, GameControllerEventArgs args)
            {
                var controllerEvent = args.ControllerEvents.First();
                if (controllerEvent.Key.EventType == GameControllerEventType.Button && 0.0F < controllerEvent.Value)
                {
                    return;
                }

                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                await alert.DismissViewControllerAsync(true);
                completionSource.SetResult(new GameControllerDialogResult { IsOk = true, EventType = controllerEvent.Key.EventType, EventCode = controllerEvent.Key.EventCode });
            }
        }

        private class InputDialogResult : IInputDialogResult
        {
            public bool IsPositive { get; set; }
            public string Result { get; set; }
        }

        private class ProgressImpl : IProgress
        {
            private readonly UIProgressView _progressView;
            private readonly int _minValue;
            private readonly int _maxValue;
            private UIAlertController _alertController;

            public ProgressImpl(UIAlertController alertController, UIProgressView progressView, int minValue, int maxValue)
            {
                _alertController = alertController;
                _progressView = progressView;
                _minValue = minValue;
                _maxValue = maxValue;
            }

            public string Title
            {
                get => _alertController.Title;
                set => _alertController.Title = value;
            }

            public string Message
            {
                get => _alertController.Message;
                set => _alertController.Message = value;
            }

            public int Progress
            {
                get => (int)(_minValue + _progressView.Progress * (_maxValue - _minValue));
                set
                {
                    if (_minValue == _maxValue)
                    {
                        return;
                    }

                    if (value < _minValue)
                    {
                        _progressView.Progress = 0;
                    }

                    if (_maxValue <= value)
                    {
                        _progressView.Progress = 1;
                    }

                    _progressView.Progress = (float)value / (_maxValue - _minValue);
                }
            }

            public async void Dispose()
            {
                if (_alertController != null)
                {
                    await _alertController.DismissViewControllerAsync(true);
                    _alertController = null;
                }
            }
        }

        private class GameControllerDialogResult : IGameControllerEventDialogResult
        {
            public bool IsOk { get; set; }
            public GameControllerEventType EventType { get; set; }
            public string EventCode { get; set; }
        }
    }
}