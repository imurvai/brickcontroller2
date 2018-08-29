using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using BrickController2.HardwareServices;
using BrickController2.UI.Services;

namespace BrickController2.Droid.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly Context _context;

        public DialogService(Context context)
        {
            _context = context;
        }

        public Task<IInputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string positiveButtonText, string negativeButtonText)
        {
            var completionSource = new TaskCompletionSource<IInputDialogResult>();

            var dialog = new Dialog(_context);
            dialog.SetContentView(Resource.Layout.InputDialog);

            var titleTextView = dialog.FindViewById<TextView>(Resource.Id.title_textview);
            titleTextView.Text = title ?? string.Empty;
            titleTextView.Visibility = string.IsNullOrEmpty(title) ? ViewStates.Gone : ViewStates.Visible;

            var messageTextView = dialog.FindViewById<TextView>(Resource.Id.message_textview);
            messageTextView.Text = message ?? string.Empty;
            messageTextView.Visibility = string.IsNullOrEmpty(message) ? ViewStates.Gone : ViewStates.Visible;

            var valueEditText = dialog.FindViewById<EditText>(Resource.Id.value_edittext);
            valueEditText.Text = initialValue ?? string.Empty;

            var positiveButton = dialog.FindViewById<Button>(Resource.Id.positive_button);
            positiveButton.Text = positiveButtonText ?? "Ok";
            positiveButton.Click += (sender, e) =>
            {
                dialog.Dismiss();
                completionSource.SetResult(new InputDialogResult { IsPositive = true, Result = valueEditText.Text });
            };

            var negativeButton = dialog.FindViewById<Button>(Resource.Id.negative_button);
            negativeButton.Text = negativeButtonText ?? "Cancel";
            negativeButton.Click += (sender, e) =>
            {
                dialog.Dismiss();
                completionSource.SetResult(new InputDialogResult { IsPositive = false, Result = valueEditText.Text });
            };

            dialog.Show();

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
            throw new NotImplementedException();
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