using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using BrickController2.Droid.HardwareServices.GameController;
using BrickController2.HardwareServices.GameController;
using BrickController2.UI.Services.Dialog;

namespace BrickController2.Droid.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly Activity _context;
        private readonly GameControllerService _gameControllerService;

        public DialogService(Activity context, GameControllerService gameControllerService)
        {
            _context = context;
            _gameControllerService = gameControllerService;
        }

        public Task ShowMessageBoxAsync(string title, string message, string buttonText)
        {
            var completionSource = new TaskCompletionSource<bool>();

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton(buttonText ?? "Ok", (sender, args) =>
                {
                    dialog.Dismiss();
                    completionSource.SetResult(true);
                })
                .Create();

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            return completionSource.Task;
        }

        public Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText)
        {
            var completionSource = new TaskCompletionSource<bool>();

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton(positiveButtonText ?? "Ok", (sender, args) =>
                {
                    dialog.Dismiss();
                    completionSource.SetResult(true);
                })
                .SetNegativeButton(negativeButtonText ?? "Cancel", (sender, args) =>
                {
                    dialog.Dismiss();
                    completionSource.SetResult(false);
                })
                .Create();

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            return completionSource.Task;
        }

        public Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText)
        {
            var completionSource = new TaskCompletionSource<InputDialogResult>();

            var inputMethodManager = (InputMethodManager)_context.GetSystemService(Context.InputMethodService);

            var view = _context.LayoutInflater.Inflate(Resource.Layout.InputDialog, null);
            var valueEditText = view.FindViewById<EditText>(Resource.Id.value_edittext);
            valueEditText.Text = initialValue ?? string.Empty;
            valueEditText.Hint = placeHolder ?? string.Empty;
            valueEditText.SetSelection(valueEditText.Text.Length);

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetView(view)
                .SetPositiveButton(positiveButtonText ?? "Ok", (sender, args) =>
                {
                    inputMethodManager.HideSoftInputFromWindow(valueEditText.ApplicationWindowToken, 0);
                    dialog.Dismiss();
                    completionSource.SetResult(new InputDialogResult(true, valueEditText.Text));
                })
                .SetNegativeButton(negativeButtonText ?? "Cancel", (sender, args) =>
                {
                    inputMethodManager.HideSoftInputFromWindow(valueEditText.ApplicationWindowToken, 0);
                    dialog.Dismiss();
                    completionSource.SetResult(new InputDialogResult(false, valueEditText.Text));
                })
                .Create();

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            valueEditText.RequestFocus();
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);

            return completionSource.Task;
        }

        public async Task ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title, string message, string cancelButtonText)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                var view = _context.LayoutInflater.Inflate(Resource.Layout.ProgressDialog, null);
                var linearLayout = view.FindViewById<LinearLayout>(Resource.Id.linearlayout);

                var progressBar = new ProgressBar(_context, null, isDeterministic ? Android.Resource.Attribute.ProgressBarStyleHorizontal : Android.Resource.Attribute.ProgressBarStyle)
                {
                    Indeterminate = !isDeterministic,
                    Progress = 0,
                    Max = 100
                };

                linearLayout.AddView(progressBar);

                var dialogBuilder = new AlertDialog.Builder(_context)
                    .SetTitle(title)
                    .SetMessage(message)
                    .SetView(view);

                if (!string.IsNullOrEmpty(cancelButtonText))
                {
                    dialogBuilder.SetNegativeButton(cancelButtonText, (sender, args) => tokenSource.Cancel());
                }

                using (var dialog = dialogBuilder.Create())
                {
                    void DialogCanceledHandler(object sender, EventArgs args) => tokenSource.Cancel();

                    dialog.CancelEvent += DialogCanceledHandler;
                    dialog.SetCancelable(false);
                    dialog.SetCanceledOnTouchOutside(false);
                    dialog.Show();

                    try
                    {
                        var progressDialog = new ProgressDialog(dialog, progressBar);
                        await action(progressDialog, tokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    finally
                    {
                        dialog.CancelEvent -= DialogCanceledHandler;
                        dialog.Dismiss();
                    }
                }
            }
        }

        public Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText)
        {
            var completionSource = new TaskCompletionSource<GameControllerEventDialogResult>();

            var dialog = new GameControllerEventDialog(_context, _gameControllerService);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetButton((int)DialogButtonType.Negative, cancelButtonText ?? "Cancel", (sender, args) =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Dismiss();
                completionSource.SetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Button, null));
            });

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            return completionSource.Task;

            void GameControllerEventHandler(object sender, GameControllerEventArgs args)
            {
                var controllerEvent = args.ControllerEvents.First();
                if (controllerEvent.Key.EventType == GameControllerEventType.Button && 0.0F < controllerEvent.Value)
                {
                    return;
                }

                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Dismiss();
                completionSource.SetResult(new GameControllerEventDialogResult(true, controllerEvent.Key.EventType, controllerEvent.Key.EventCode));
            }
        }
    }
}