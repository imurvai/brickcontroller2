using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class SequenceEditorPageViewModel : PageViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ICreationManager _creationManager;

        private CancellationTokenSource _disappearingTokenSource;

        public SequenceEditorPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDialogService dialogService,
            ICreationManager creationManager,
            NavigationParameters parameters) :
            base(navigationService, translationService)
        {
            _dialogService = dialogService;
            _creationManager = creationManager;

            OriginalSequence = parameters.Get<Sequence>("sequence");

            Sequence = new Sequence
            {
                Name = OriginalSequence.Name,
                Loop = OriginalSequence.Loop,
                Interpolate = OriginalSequence.Interpolate,
                ControlPoints = new ObservableCollection<SequenceControlPoint>(OriginalSequence.ControlPoints.Select(cp => new SequenceControlPoint { Value = cp.Value, DurationMs = cp.DurationMs }).ToArray())
            };

            RenameSequenceCommand = new SafeCommand(async () => await RenameSequenceAsync());
            AddControlPointCommand = new SafeCommand(() => AddControlPoint());
            DeleteControlPointCommand = new SafeCommand<SequenceControlPoint>(async (controlPoint) => await DeleteControlPointAsync(controlPoint));
            SaveSequenceCommand = new SafeCommand(async () => await SaveSequenceAsync(), () => !_dialogService.IsDialogOpen);
            ChangeControlPointDurationCommand = new SafeCommand<SequenceControlPoint>(async (controlPoint) => await ChangeControlPointDurationAsync(controlPoint));
        }

        public Sequence OriginalSequence { get; }
        public Sequence Sequence { get; }

        public ICommand RenameSequenceCommand { get; }
        public ICommand AddControlPointCommand { get; }
        public ICommand DeleteControlPointCommand { get; }
        public ICommand SaveSequenceCommand { get; }
        public ICommand ChangeControlPointDurationCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource?.Cancel();
        }

        private async Task RenameSequenceAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Sequence.Name,
                    Translate("SequenceName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (value) => !string.IsNullOrEmpty(value),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("SequenceNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }
                    else if (!(await _creationManager.IsSequenceNameAvailableAsync(result.Result)))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("SequenceNameIsUsed"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    Sequence.Name = result.Result;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void AddControlPoint()
        {
            var controlPoint = new SequenceControlPoint { Value = 0, DurationMs = 1000 };
            Sequence.ControlPoints.Add(controlPoint);
        }

        private async Task DeleteControlPointAsync(SequenceControlPoint controlPoint)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    Translate("AreYouSureToDeleteControlPoint"),
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    Sequence.ControlPoints.Remove(controlPoint);

                    // This hack is needed to rebuild the bindings properly in the listview
                    Sequence.ControlPoints = new ObservableCollection<SequenceControlPoint>(Sequence.ControlPoints);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ChangeControlPointDurationAsync(SequenceControlPoint controlPoint)
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    controlPoint.DurationMs.ToString(),
                    Translate("Value"),
                    Translate("Ok"),
                    Translate("Cancel"),
                    KeyboardType.Numeric,
                    (durationText) => !string.IsNullOrEmpty(durationText) && int.TryParse(durationText, out int durationMs) && durationMs >= 300 && durationMs <= 10000,
                    _disappearingTokenSource.Token);

                if (!result.IsOk)
                {
                    return;
                }

                if (int.TryParse(result.Result, out int intValue))
                {
                    if (intValue < 300 || 10000 < intValue)
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warining"),
                            Translate("ValueOutOfRange"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    controlPoint.DurationMs = intValue;
                }
                else
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ValueMustBeNumeric"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);

                    return;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task SaveSequenceAsync()
        {
            try
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) =>
                    {
                        await _creationManager.UpdateSequenceAsync(
                            OriginalSequence,
                            Sequence.Name,
                            Sequence.Loop,
                            Sequence.Interpolate,
                            Sequence.ControlPoints);
                    },
                    Translate("Saving"));

                await NavigationService.NavigateBackAsync();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
