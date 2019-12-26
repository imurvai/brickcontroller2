﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using BrickController2.Droid.PlatformServices.GameController;
using BrickController2.PlatformServices.GameController;
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

        public async Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton(buttonText ?? "Ok", (sender, args) =>
                {
                    dialog.Dismiss();
                    completionSource.TrySetResult(true);
                })
                .Create();

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            using (token.Register(() =>
            {
                dialog.Dismiss();
                completionSource.TrySetResult(true);
            }))
            {
                await completionSource.Task;
            }
        }

        public async Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetPositiveButton(positiveButtonText ?? "Ok", (sender, args) =>
                {
                    dialog.Dismiss();
                    completionSource.TrySetResult(true);
                })
                .SetNegativeButton(negativeButtonText ?? "Cancel", (sender, args) =>
                {
                    dialog.Dismiss();
                    completionSource.TrySetResult(false);
                })
                .Create();

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            using (token.Register(() =>
            {
                dialog.Dismiss();
                completionSource.TrySetCanceled();
            }))
            {
                return await completionSource.Task;
            }
        }

        public async Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, KeyboardType keyboardType, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<InputDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            var inputMethodManager = (InputMethodManager)_context.GetSystemService(Context.InputMethodService);

            var view = _context.LayoutInflater.Inflate(Resource.Layout.InputDialog, null);
            var valueEditText = view.FindViewById<EditText>(Resource.Id.value_edittext);
            valueEditText.Text = initialValue ?? string.Empty;
            valueEditText.Hint = placeHolder ?? string.Empty;
            valueEditText.SetSelection(valueEditText.Text.Length);
            valueEditText.SetRawInputType(KeyboardTypeToRawInputType(keyboardType));

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetView(view)
                .SetPositiveButton(positiveButtonText ?? "Ok", (sender, args) =>
                {
                    inputMethodManager.HideSoftInputFromWindow(valueEditText.ApplicationWindowToken, 0);
                    dialog.Dismiss();
                    completionSource.TrySetResult(new InputDialogResult(true, valueEditText.Text));
                })
                .SetNegativeButton(negativeButtonText ?? "Cancel", (sender, args) =>
                {
                    inputMethodManager.HideSoftInputFromWindow(valueEditText.ApplicationWindowToken, 0);
                    dialog.Dismiss();
                    completionSource.TrySetResult(new InputDialogResult(false, valueEditText.Text));
                })
                .Create();

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            valueEditText.RequestFocus();
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);

            using (token.Register(() =>
            {
                dialog.Dismiss();
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

        public async Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token)
        {
            var completionSource = new TaskCompletionSource<GameControllerEventDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            var dialog = new GameControllerEventDialog(_context, _gameControllerService);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetButton((int)DialogButtonType.Negative, cancelButtonText ?? "Cancel", (sender, args) =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Dismiss();
                completionSource.TrySetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Button, null));
            });

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            using (token.Register(() =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Dismiss();
                completionSource.TrySetCanceled();
            }))
            {
                return await completionSource.Task;
            }

            void GameControllerEventHandler(object sender, GameControllerEventArgs args)
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
                        dialog.Dismiss();
                        completionSource.TrySetResult(new GameControllerEventDialogResult(true, controllerEvent.Key.EventType, controllerEvent.Key.EventCode));
                        return;
                    }
                }
            }
        }

        private InputTypes KeyboardTypeToRawInputType(KeyboardType keyboardType)
        {
            switch (keyboardType)
            {
                case KeyboardType.Dialer:
                    return InputTypes.ClassPhone;

                case KeyboardType.Email:
                    return InputTypes.TextVariationEmailAddress;

                case KeyboardType.Numeric:
                    return InputTypes.ClassNumber;

                default:
                    return InputTypes.ClassText;
            }
        }
    }
}