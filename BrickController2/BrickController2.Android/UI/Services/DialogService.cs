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

        public Task<IProgress> ShowProgressDialogAsync(string title, string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue)
        {
            throw new NotImplementedException();
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
            private Dialog _progressDialog;
            private ProgressBar _progressBar;

            public ProgressImpl(Dialog progressDialog, ProgressBar progressBar)
            {
                _progressDialog = progressDialog;
                _progressBar = progressBar;
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