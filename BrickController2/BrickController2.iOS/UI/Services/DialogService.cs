using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.Dialog;
using CoreGraphics;
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

        public async Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create(buttonText ?? "Ok", UIAlertActionStyle.Default, action =>
            {
                completionSource.TrySetResult(true);
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            using (token.Register(async () =>
            {
                await alert.DismissViewControllerAsync(true);
                completionSource.TrySetResult(true);
            }))
            {
                await completionSource.Task;
            }
        }

        public async Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create(positiveButtonText ?? "Ok", UIAlertActionStyle.Default, action =>
            {
                completionSource.TrySetResult(true);
            }));

            alert.AddAction(UIAlertAction.Create(negativeButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                completionSource.TrySetResult(false);
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            using (token.Register(async () =>
            {
                await alert.DismissViewControllerAsync(true);
                completionSource.TrySetCanceled();
            }))
            {
                return await completionSource.Task;
            }
        }

        public async Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, KeyboardType keyboardType, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<InputDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            alert.AddTextField(textField =>
            {
                textField.Text = initialValue ?? string.Empty;
                textField.Placeholder = placeHolder ?? string.Empty;
                textField.KeyboardType = KeyboardTypeToUIKeyboardType(keyboardType);
            });

            alert.AddAction(UIAlertAction.Create(positiveButtonText ?? "Ok", UIAlertActionStyle.Default, action =>
            {
                completionSource.TrySetResult(new InputDialogResult(true, alert.TextFields.First().Text));
            }));

            alert.AddAction(UIAlertAction.Create(negativeButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                completionSource.TrySetResult(new InputDialogResult(false, alert.TextFields.First().Text));
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            using (token.Register(async () =>
            {
                await alert.DismissViewControllerAsync(true);
                completionSource.TrySetCanceled();
            }))
            {
                return await completionSource.Task;
            }
        }

        public async Task ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title, string message, string cancelButtonText)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                message = string.IsNullOrEmpty(message) ? "\n\n" : message + "\n\n";

                using (var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert))
                {
                    UIProgressView progressView = null;
                    if (isDeterministic)
                    {
                        progressView = new UIProgressView(new CGRect(30F, 80F, 200F, 2F));
                        progressView.Style = UIProgressViewStyle.Bar;
                        progressView.Progress = 0.0F;
                        alert.View.AddSubview(progressView);
                    }
                    else
                    {
                        var activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray);
                        activityIndicator.Frame = new CGRect(121F, 56F, 37F, 37F);
                        activityIndicator.StartAnimating();
                        alert.View.AddSubview(activityIndicator);
                    }

                    if (!string.IsNullOrEmpty(cancelButtonText))
                    {
                        alert.AddAction(UIAlertAction.Create(cancelButtonText, UIAlertActionStyle.Cancel, _ => tokenSource.Cancel()));
                    }

                    UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

                    try
                    {
                        var progressDialog = new ProgressDialog(alert, progressView);
                        var cancelationToken = tokenSource.Token;
                        await action(progressDialog, cancelationToken);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        if (!tokenSource.IsCancellationRequested)
                        {
                            await alert.DismissViewControllerAsync(true);
                        }
                    }
                }
            }
        }

        public async Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<GameControllerEventDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            alert.AddAction(UIAlertAction.Create(cancelButtonText ?? "Cancel", UIAlertActionStyle.Cancel, action =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                completionSource.TrySetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Button, null));
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alert, true, null);

            using (token.Register(async () =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                await alert.DismissViewControllerAsync(true);
                completionSource.TrySetCanceled();
            }))
            {
                return await completionSource.Task;
            }

            async void GameControllerEventHandler(object sender, GameControllerEventArgs args)
            {
                if (args.ControllerEvents.Count == 0)
                {
                    return;
                }

                foreach (var controllerEvent in args.ControllerEvents)
                {
                    if ((controllerEvent.Key.EventType == GameControllerEventType.Axis && Math.Abs(controllerEvent.Value) > 0.8) ||
                        (controllerEvent.Key.EventType == GameControllerEventType.Button && Math.Abs(controllerEvent.Value) < 0.05))
                    {
                        _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                        await alert.DismissViewControllerAsync(true);
                        completionSource.TrySetResult(new GameControllerEventDialogResult(true, controllerEvent.Key.EventType, controllerEvent.Key.EventCode));
                        return;
                    }
                }
            }
        }

        private UIKeyboardType KeyboardTypeToUIKeyboardType(KeyboardType keyboardType)
        {
            switch (keyboardType)
            {
                case KeyboardType.Dialer:
                    return UIKeyboardType.PhonePad;

                case KeyboardType.Email:
                    return UIKeyboardType.EmailAddress;

                case KeyboardType.Numeric:
                    return UIKeyboardType.DecimalPad;

                default:
                    return UIKeyboardType.Default;
            }
        }
    }
}