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

        public Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText)
        {
            var completionSource = new TaskCompletionSource<InputDialogResult>();
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            alert.AddTextField(textField =>
            {
                textField.Text = initialValue ?? string.Empty;
                textField.Placeholder = placeHolder ?? string.Empty;
            });

            alert.AddAction(UIAlertAction.Create(positiveButtonText ?? "Ok", UIAlertActionStyle.Default, action =>
            {
                completionSource.SetResult(new InputDialogResult(true, alert.TextFields.First().Text));
            }));

            alert.AddAction(UIAlertAction.Create(negativeButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                completionSource.SetResult(new InputDialogResult(false, alert.TextFields.First().Text));
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            return completionSource.Task;
        }

        public IProgress ShowProgressDialog(string title, string message, string cancelButtonText, CancellationTokenSource tokenSource, int maxValue)
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

            return new ProgressImpl(alert, progressView, maxValue);
        }

        public Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText)
        {
            var completionSource = new TaskCompletionSource<GameControllerEventDialogResult>();
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            alert.AddAction(UIAlertAction.Create(cancelButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                completionSource.SetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Button, null));
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
                completionSource.SetResult(new GameControllerEventDialogResult(true, controllerEvent.Key.EventType, controllerEvent.Key.EventCode));
            }
        }
    }
}