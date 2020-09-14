using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dialogs : ContentView, IDialogServer
    {
        public IGameControllerService GameControllerService { get; set; }

        public Dialogs()
        {
            InitializeComponent();

            IsVisible = false;
            MessageBox.IsVisible = false;
            QuestionDialog.IsVisible = false;
            InputDialog.IsVisible = false;
            ProgressDialog.IsVisible = false;
            GameControllerEventDialog.IsVisible = false;
        }

        public bool IsDialogOpen =>
            MessageBox.IsVisible ||
            QuestionDialog.IsVisible ||
            InputDialog.IsVisible ||
            SelectionDialog.IsVisible ||
            ProgressDialog.IsVisible ||
            GameControllerEventDialog.IsVisible ||
            SequenceInputDialog.IsVisible;

        public async Task ShowMessageBoxAsync(string title, string message, string buttonText, CancellationToken token)
        {
            MessageBoxTitle.Text = title ?? string.Empty;
            MessageBoxMessage.Text = message ?? string.Empty;
            MessageBoxButton.Text = buttonText ?? "Ok";

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                MessageBoxButton.Clicked -= buttonHandler;
                MessageBox.IsVisible = false;
                HideViewImmediately(MessageBox);
                tcs.TrySetResult(true);
            }))
            {
                await ShowView(MessageBox);
                MessageBoxButton.Clicked += buttonHandler;

                await tcs.Task;
            }

            async void buttonHandler(object sender, EventArgs args)
            {
                MessageBoxButton.Clicked -= buttonHandler;
                await HideView(MessageBox);
                tcs.TrySetResult(true);
            }
        }

        public async Task<bool> ShowQuestionDialogAsync(string title, string message, string positiveButtonText, string negativeButtonText, CancellationToken token)
        {
            QuestionDialogTitle.Text = title ?? string.Empty;
            QuestionDialogMessage.Text = message ?? string.Empty;
            QuestionDialogPositiveButton.Text = positiveButtonText ?? "Yes";
            QuestionDialogNegativeButton.Text = negativeButtonText ?? "No";

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                QuestionDialogPositiveButton.Clicked -= buttonHandler;
                QuestionDialogNegativeButton.Clicked -= buttonHandler;
                QuestionDialog.IsVisible = false;
                HideViewImmediately(QuestionDialog);
                tcs.TrySetResult(false);
            }))
            {
                await ShowView(QuestionDialog);
                QuestionDialogPositiveButton.Clicked += buttonHandler;
                QuestionDialogNegativeButton.Clicked += buttonHandler;

                return await tcs.Task;
            }

            async void buttonHandler(object sender, EventArgs args)
            {
                QuestionDialogPositiveButton.Clicked -= buttonHandler;
                QuestionDialogNegativeButton.Clicked -= buttonHandler;
                await HideView(QuestionDialog);
                tcs.TrySetResult(sender == QuestionDialogPositiveButton);
            }
        }

        public async Task<InputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string placeHolder, string positiveButtonText, string negativeButtonText, KeyboardType keyboardType, Predicate<string> valuePredicate, CancellationToken token)
        {
            InputDialogTitle.Text = title ?? string.Empty;
            InputDialogMessage.Text = message ?? string.Empty;
            InputDialogEntry.Text = initialValue ?? string.Empty;
            InputDialogEntry.Placeholder = placeHolder ?? string.Empty;
            InputDialogEntry.CursorPosition = InputDialogEntry.Text.Length;
            InputDialogPositiveButton.Text = positiveButtonText ?? "Ok";
            InputDialogPositiveButton.IsEnabled = valuePredicate == null || valuePredicate(InputDialogEntry.Text);
            InputDialogNegativeButton.Text = negativeButtonText ?? "Cancel";

            var tcs = new TaskCompletionSource<InputDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                InputDialogEntry.TextChanged -= entryTextChanged;
                InputDialogPositiveButton.Clicked -= buttonHandler;
                InputDialogNegativeButton.Clicked -= buttonHandler;
                HideViewImmediately(InputDialog);
                tcs.TrySetResult(new InputDialogResult(false, InputDialogEntry.Text));
            }))
            {
                await ShowView(InputDialog);
                InputDialogEntry.TextChanged += entryTextChanged;
                InputDialogPositiveButton.Clicked += buttonHandler;
                InputDialogNegativeButton.Clicked += buttonHandler;
                InputDialogEntry.Keyboard = GetKeyboard(keyboardType);
                InputDialogEntry.Focus();

                return await tcs.Task;
            }

            async void buttonHandler(object sender, EventArgs args)
            {
                InputDialogEntry.TextChanged -= entryTextChanged;
                InputDialogPositiveButton.Clicked -= buttonHandler;
                InputDialogNegativeButton.Clicked -= buttonHandler;
                await HideView(InputDialog);
                tcs.TrySetResult(new InputDialogResult(sender == InputDialogPositiveButton, InputDialogEntry.Text));
            }

            void entryTextChanged(object sender, EventArgs args)
            {
                InputDialogPositiveButton.IsEnabled = valuePredicate == null || valuePredicate(InputDialogEntry.Text);
            }
        }

        public async Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items, string title, string cancelButtonText, CancellationToken token)
        {
            SelectionDialogTitle.Text = title ?? string.Empty;
            SelectionDialogItems.ItemsSource = items;
            SelectionDialogItems.SelectedItem = null;
            SelectionDialogCancelButton.Text = cancelButtonText ?? "Cancel";

            var tcs = new TaskCompletionSource<SelectionDialogResult<T>>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                SelectionDialogItems.SelectionChanged -= selectionChangedHandler;
                SelectionDialogCancelButton.Clicked -= buttonHandler;
                HideViewImmediately(SelectionDialog);
                tcs.TrySetResult(new SelectionDialogResult<T>(false, default));
            }))
            {
                await ShowView(SelectionDialog);
                SelectionDialogItems.SelectionChanged += selectionChangedHandler;
                SelectionDialogCancelButton.Clicked += buttonHandler;

                return await tcs.Task;
            }

            async void selectionChangedHandler(object sender, EventArgs args)
            {
                SelectionDialogItems.SelectionChanged -= selectionChangedHandler;
                SelectionDialogCancelButton.Clicked -= buttonHandler;
                await HideView(SelectionDialog);
                tcs.TrySetResult(new SelectionDialogResult<T>(true, (T)SelectionDialogItems.SelectedItem));
            }

            async void buttonHandler(object sender, EventArgs args)
            {
                SelectionDialogItems.SelectionChanged -= selectionChangedHandler;
                SelectionDialogCancelButton.Clicked -= buttonHandler;
                await HideView(SelectionDialog);
                tcs.TrySetResult(new SelectionDialogResult<T>(false, default));
            }
        }

        public async Task ShowProgressDialogAsync(bool isDeterministic, Func<IProgressDialog, CancellationToken, Task> action, string title = null, string message = null, string cancelButtonText = null)
        {
            ProgressDialogTitle.Text = title ?? string.Empty;
            ProgressDialogTitle.IsVisible = !string.IsNullOrEmpty(title);
            ProgressDialogMessage.Text = message ?? string.Empty;
            ProgressDialogMessage.IsVisible = !string.IsNullOrEmpty(message);
            ProgressDialogCancelButton.Text = cancelButtonText ?? string.Empty;
            ProgressBarButtonPart.IsVisible = !string.IsNullOrEmpty(cancelButtonText);
            ProgressDialogProgressBar.Progress = 0.0;
            ProgressDialogProgressBar.IsVisible = isDeterministic;
            ProgressDialogActivityIndicator.IsVisible = !isDeterministic;

            using (var tokenSource = new CancellationTokenSource())
            {
                await ShowView(ProgressDialog);
                ProgressDialogCancelButton.Clicked += buttonHandler;

                try
                {
                    var progressDialog = new ProgressDialogImpl(ProgressDialogTitle, ProgressDialogMessage, ProgressDialogProgressBar);
                    await action(progressDialog, tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    ProgressDialogCancelButton.Clicked -= buttonHandler;
                    await HideView(ProgressDialog);
                }

                void buttonHandler(object sender, EventArgs args)
                {
                    tokenSource.Cancel();
                }
            }
        }

        public async Task<GameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText, CancellationToken token)
        {
            GameControllerEventDialogTitle.Text = title ?? string.Empty;
            GameControllerEventDialogMessage.Text = message ?? string.Empty;
            GameControllerEventDialogCancelButton.Text = cancelButtonText ?? string.Empty;

            var tcs = new TaskCompletionSource<GameControllerEventDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                GameControllerEventDialogCancelButton.Clicked -= buttonHandler;
                GameControllerService.GameControllerEvent -= gameControllerEventHandler;
                HideViewImmediately(GameControllerEventDialog);
                tcs.TrySetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Axis, string.Empty));
            }))
            {
                await ShowView(GameControllerEventDialog);
                GameControllerEventDialogCancelButton.Clicked += buttonHandler;
                GameControllerService.GameControllerEvent += gameControllerEventHandler;

                return await tcs.Task;
            }

            async void buttonHandler(object sender, EventArgs args)
            {
                GameControllerEventDialogCancelButton.Clicked -= buttonHandler;
                GameControllerService.GameControllerEvent -= gameControllerEventHandler;
                await HideView(GameControllerEventDialog);
                tcs.TrySetResult(new GameControllerEventDialogResult(false, GameControllerEventType.Axis, string.Empty));
            }

            async void gameControllerEventHandler(object sender, GameControllerEventArgs args)
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
                        GameControllerEventDialogCancelButton.Clicked -= buttonHandler;
                        GameControllerService.GameControllerEvent -= gameControllerEventHandler;
                        await HideView(GameControllerEventDialog);

                        var gameControllerEventType = controllerEvent.Key.EventType;
                        var gameControllerEventCode = controllerEvent.Key.EventCode;
                        tcs.TrySetResult(new GameControllerEventDialogResult(true, gameControllerEventType, gameControllerEventCode));
                        return;
                    }
                }
            }
        }

        public async Task<SequenceInputDialogResult> ShowSequenceInputDialogAsync(string title, string message, string valueText, float value, string durationText, int durationMs, string positiveButtonText, string negativeButtonText, Predicate<string> durationPredicate, CancellationToken token)
        {
            SequenceInputDialogTitle.Text = title ?? string.Empty;
            SequenceInputDialogMessage.Text = message ?? string.Empty;
            SequenceInputDialogValueTextLabel.Text = valueText ?? string.Empty;
            SequenceInputDialogValueSlider.Value = value * 100;
            SequenceInputDialogValueLabel.Text = (value * 100).ToString();
            SequenceInputDialogDurationMsText.Text = durationText ?? string.Empty;
            SequenceInputDialogDurationMsEntry.Text = durationMs.ToString();
            SequenceInputDialogDurationMsEntry.CursorPosition = SequenceInputDialogDurationMsEntry.Text.Length;
            SequenceInputDialogPositiveButton.IsEnabled = durationPredicate == null || durationPredicate(SequenceInputDialogDurationMsEntry.Text);
            SequenceInputDialogPositiveButton.Text = positiveButtonText ?? "Ok";
            SequenceInputDialogNegativeButton.Text = negativeButtonText ?? "Cancel";

            var tcs = new TaskCompletionSource<SequenceInputDialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (token.Register(() =>
            {
                SequenceInputDialogValueSlider.ValueChanged -= sliderValueChanged;
                SequenceInputDialogPositiveButton.Clicked -= buttonHandler;
                SequenceInputDialogNegativeButton.Clicked -= buttonHandler;
                SequenceInputDialogDurationMsEntry.TextChanged -= entryTextChanged;
                SequenceInputDialogDurationMsEntry.Completed -= buttonHandler;
                HideViewImmediately(SequenceInputDialog);
                tcs.TrySetResult(new SequenceInputDialogResult(false, 0, 0));
            }))
            {
                await ShowView(SequenceInputDialog);
                SequenceInputDialogValueSlider.ValueChanged += sliderValueChanged;
                SequenceInputDialogPositiveButton.Clicked += buttonHandler;
                SequenceInputDialogNegativeButton.Clicked += buttonHandler;
                SequenceInputDialogDurationMsEntry.TextChanged += entryTextChanged;
                SequenceInputDialogDurationMsEntry.Completed += buttonHandler;
                SequenceInputDialogDurationMsEntry.Keyboard = Keyboard.Numeric;
                SequenceInputDialogDurationMsEntry.Focus();

                return await tcs.Task;
            }

            async void buttonHandler(object sender, EventArgs args)
            {
                SequenceInputDialogValueSlider.ValueChanged -= sliderValueChanged;
                SequenceInputDialogPositiveButton.Clicked -= buttonHandler;
                SequenceInputDialogNegativeButton.Clicked -= buttonHandler;
                SequenceInputDialogDurationMsEntry.TextChanged -= entryTextChanged;
                SequenceInputDialogDurationMsEntry.Completed -= buttonHandler;
                await HideView(SequenceInputDialog);
                tcs.TrySetResult(new SequenceInputDialogResult(sender == SequenceInputDialogPositiveButton || sender == SequenceInputDialogDurationMsEntry, (float)(SequenceInputDialogValueSlider.Value / 100), int.Parse(SequenceInputDialogDurationMsEntry.Text)));
            }

            void sliderValueChanged(object sender, EventArgs args)
            {
                SequenceInputDialogValueLabel.Text = SequenceInputDialogValueSlider.Value.ToString();
            }

            void entryTextChanged(object sender, EventArgs args)
            {
                SequenceInputDialogPositiveButton.IsEnabled = durationPredicate == null || durationPredicate(SequenceInputDialogDurationMsEntry.Text);
            }
        }

        private async Task ShowView(View view)
        {
            IsVisible = true;
            view.Scale = 1.0;
            view.Opacity = 0.0;
            view.IsVisible = true;

            await Task.WhenAll(new Task[]
            {
                view.ScaleTo(1.2, 50),
                view.FadeTo(1.0, 50)
            });

            await view.ScaleTo(1.0, 50);
        }

        private async Task HideView(View view)
        {
            await view.ScaleTo(1.2, 50);

            await Task.WhenAll(new Task[]
            {
                view.ScaleTo(1.0, 50),
                view.FadeTo(0, 50)
            });

            HideViewImmediately(view);
        }

        private void HideViewImmediately(View view)
        {
            view.IsVisible = false;
            IsVisible = false;
        }

        private Keyboard GetKeyboard(KeyboardType keyboardType)
        {
            return keyboardType switch
            {
                KeyboardType.Dialer => Keyboard.Telephone,
                KeyboardType.Email => Keyboard.Email,
                KeyboardType.Numeric => Keyboard.Numeric,
                KeyboardType.Text => Keyboard.Text,
                _ => throw new ArgumentOutOfRangeException(nameof(keyboardType)),
            };
        }

        private class ProgressDialogImpl : IProgressDialog
        {
            private readonly Label _title;
            private readonly Label _message;
            private readonly ProgressBar _progressBar;

            public ProgressDialogImpl(Label title, Label message, ProgressBar progressBar)
            {
                _title = title;
                _message = message;
                _progressBar = progressBar;
            }

            public string Title { set => _title.Text = value; }
            public string Message { set => _message.Text = value; }
            public int Percent
            {
                get => (int)(_progressBar.Progress * 100);
                set => _progressBar.Progress = (value / 100.0);
            }
        }
    }
}