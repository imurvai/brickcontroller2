using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;
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

        public Task<IInputDialogResult> ShowInputDialogAsync(string message, string initialValue, string positiveButtonText, string negativeButtonText)
        {
            var dialog = new Dialog(_context);
            dialog.SetContentView(Resource.Layout.InputDialog);

            var positiveButton = dialog.FindViewById<Button>(Resource.Id.positive_button);
            var negativeButton = dialog.FindViewById<Button>(Resource.Id.negative_button);
            var messageTextView = dialog.FindViewById<TextView>(Resource.Id.message_textview);
            var valueEditText = dialog.FindViewById<EditText>(Resource.Id.value_edittext);

            messageTextView.Text = message ?? string.Empty;
            valueEditText.Text = initialValue ?? string.Empty;

            var completionSource = new TaskCompletionSource<IInputDialogResult>();

            positiveButton.Text = positiveButtonText ?? "Ok";
            positiveButton.Click += (sender, e) =>
            {
                dialog.Dismiss();
                completionSource.SetResult(new InputDialogResult { IsPositive = true, Result = valueEditText.Text });
            };

            negativeButton.Text = negativeButtonText ?? "Cancel";
            negativeButton.Click += (sender, e) =>
            {
                dialog.Dismiss();
                completionSource.SetResult(new InputDialogResult { IsPositive = false, Result = valueEditText.Text });
            };

            dialog.Show();

            return completionSource.Task;
        }

        public Task<IDisposable> ShowProgressDialogAsync(string message, string cancelButtonText, CancellationTokenSource tokenSource)
        {
            throw new NotImplementedException();
        }

        public Task<IDeterministicProgressDialog> ShowProgressDialogAsync(string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue)
        {
            throw new NotImplementedException();
        }

        public Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string message, string cancelButtonText)
        {
            throw new NotImplementedException();
        }
    }
}