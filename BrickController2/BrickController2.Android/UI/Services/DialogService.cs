using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using BrickController2.HardwareServices;
using BrickController2.UI.Services;

namespace BrickController2.Droid.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly Activity _context;
        private readonly IGameControllerService _gameControllerService;

        public DialogService(Activity context, IGameControllerService gameControllerService)
        {
            _context = context;
            _gameControllerService = gameControllerService;
        }

        public Task<IInputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText)
        {
            var completionSource = new TaskCompletionSource<IInputDialogResult>();

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
                    completionSource.SetResult(new InputDialogResult { IsPositive = true, Result = valueEditText.Text });
                })
                .SetNegativeButton(negativeButtonText ?? "Cancel", (sender, args) =>
                {
                    inputMethodManager.HideSoftInputFromWindow(valueEditText.ApplicationWindowToken, 0);
                    dialog.Dismiss();
                    completionSource.SetResult(new InputDialogResult { IsPositive = false, Result = valueEditText.Text });
                })
                .Create();

            dialog.Show();

            valueEditText.RequestFocus();
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);

            return completionSource.Task;
        }

        public IProgress ShowProgressDialogAsync(string title, string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue)
        {
            var dialog = new Dialog(_context);
            dialog.SetContentView(Resource.Layout.ProgressDialog);

            var titleTextView = dialog.FindViewById<TextView>(Resource.Id.title_textview);
            titleTextView.Text = title ?? string.Empty;
            titleTextView.Visibility = string.IsNullOrEmpty(title) ? ViewStates.Gone : ViewStates.Visible;

            var messageTextView = dialog.FindViewById<TextView>(Resource.Id.message_textview);
            messageTextView.Text = message ?? string.Empty;
            messageTextView.Visibility = string.IsNullOrEmpty(message) ? ViewStates.Gone : ViewStates.Visible;

            var progressBar = dialog.FindViewById<ProgressBar>(Resource.Id.progressbar);
            progressBar.Min = minValue;
            progressBar.Max = maxValue;
            progressBar.Progress = 0;
            progressBar.Indeterminate = minValue == maxValue;

            var cancelButton = dialog.FindViewById<Button>(Resource.Id.cancel_button);
            cancelButton.Text = cancelButtonText ?? "Cancel";
            cancelButton.Visibility = tokenSource != null ? ViewStates.Visible : ViewStates.Gone;
            cancelButton.Click += (sender, e) =>
            {
                dialog.Dismiss();
                tokenSource?.Cancel();
            };

            return new ProgressImpl(dialog, titleTextView, messageTextView, progressBar);
        }

        public Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText)
        {
            // TODO: fix this method!!!
            var completionSource = new TaskCompletionSource<IGameControllerEventDialogResult>();

            AlertDialog dialog = null;
            dialog = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetNegativeButton(cancelButtonText ?? "Cancel", (sender, args) =>
                {
                    _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                    dialog.Dismiss();
                    completionSource.SetResult(new GameControllerDialogResult { IsOk = false, EventType = GameControllerEventType.Button, EventCode = null });
                })
                .Create();

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

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
            private readonly TextView _title;
            private readonly TextView _message;
            private readonly ProgressBar _progressBar;
            private Dialog _progressDialog;

            public ProgressImpl(Dialog progressDialog, TextView title, TextView message, ProgressBar progressBar)
            {
                _progressDialog = progressDialog;
                _title = title;
                _message = message;
                _progressBar = progressBar;
            }

            public string Title
            {
                get => _title.Text;
                set => _title.Text = value ?? string.Empty;
            }

            public string Message
            {
                get => _message.Text;
                set => _message.Text = value ?? string.Empty;
            }

            public int Progress
            {
                get => _progressBar.Progress;
                set => _progressBar.Progress = Math.Min(_progressBar.Max, Math.Max(_progressBar.Min, value));
            }

            public void Dispose()
            {
                if (_progressDialog != null)
                {
                    _progressDialog.Dismiss();
                    _progressDialog = null;
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