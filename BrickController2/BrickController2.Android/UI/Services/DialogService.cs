using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using BrickController2.Droid.HardwareServices;
using BrickController2.HardwareServices;
using BrickController2.UI.Services;

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
            var view = _context.LayoutInflater.Inflate(Resource.Layout.ProgressDialog, null);
            var progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressbar);

            progressBar.Indeterminate = minValue == maxValue;
            progressBar.Min = minValue;
            progressBar.Max = maxValue;
            progressBar.Progress = minValue;

            var dialogBuilder = new AlertDialog.Builder(_context)
                .SetTitle(title)
                .SetMessage(message)
                .SetView(view);

            if (tokenSource != null)
            {
                dialogBuilder.SetNegativeButton(cancelButtonText ?? "Cancel", (sender, args) => tokenSource.Cancel());
            }
                
            var dialog = dialogBuilder.Create();
            dialog.Show();

            return new ProgressImpl(dialog, progressBar);
        }

        public Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText)
        {
            var completionSource = new TaskCompletionSource<IGameControllerEventDialogResult>();

            var dialog = new GameControllerEventDialog(_context, _gameControllerService);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetButton((int)DialogButtonType.Negative, cancelButtonText ?? "Cancel", (sender, args) =>
            {
                _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
                dialog.Dismiss();
                completionSource.SetResult(new GameControllerEventDialogResult { IsOk = false, EventType = GameControllerEventType.Button, EventCode = null });
            });

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
                completionSource.SetResult(new GameControllerEventDialogResult { IsOk = true, EventType = controllerEvent.Key.EventType, EventCode = controllerEvent.Key.EventCode });
            }
        }
    }
}