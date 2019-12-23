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
            DeleteControlPointCommand = new SafeCommand<SequenceControlPoint>(async (controlPoint) => await DeleceControlPointAsync(controlPoint));
            SaveSequenceCommand = new SafeCommand(async () => await SaveSequenceAsync());
            ControlPointTappedCommand = new SafeCommand<SequenceControlPoint>(async (controlPoint) => await EditControlPoint(controlPoint));
        }

        public Sequence OriginalSequence { get; }
        public Sequence Sequence { get; }

        public ICommand RenameSequenceCommand { get; }
        public ICommand AddControlPointCommand { get; }
        public ICommand DeleteControlPointCommand { get; }
        public ICommand SaveSequenceCommand { get; }
        public ICommand ControlPointTappedCommand { get; }

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
            var result = await _dialogService.ShowInputDialogAsync(
                Translate("Sequence"),
                Translate("EnterSequenceName"),
                Sequence.Name,
                Translate("SequenceName"),
                Translate("Rename"),
                Translate("Cancel"),
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

        private void AddControlPoint()
        {
            Sequence.ControlPoints.Add(new SequenceControlPoint { Value = 0, DurationMs = 100 });
        }

        private async Task DeleceControlPointAsync(SequenceControlPoint controlPoint)
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
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task EditControlPoint(SequenceControlPoint controlPoint)
        {
            await _dialogService.ShowMessageBoxAsync("title", "edit", "ok", _disappearingTokenSource.Token);
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
