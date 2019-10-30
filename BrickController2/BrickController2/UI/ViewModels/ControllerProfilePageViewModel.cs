using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using BrickController2.DeviceManagement;
using System.Collections.ObjectModel;
using System;
using System.Threading;
using BrickController2.UI.Services.Translation;
using System.Linq;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfilePageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _disappearingTokenSource;

        public ControllerProfilePageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");

            RenameProfileCommand = new SafeCommand(async () => await RenameControllerProfileAsync());
            AddControllerEventCommand = new SafeCommand(async () => await AddControllerEventAsync());
            ControllerActionTappedCommand = new SafeCommand<ControllerActionViewModel>(async controllerActionViewModel => await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controlleraction", controllerActionViewModel.ControllerAction))));
            DeleteControllerEventCommand = new SafeCommand<ControllerEvent>(async controllerEvent => await DeleteControllerEventAsync(controllerEvent));
            DeleteControllerActionCommand = new SafeCommand<ControllerAction>(async controllerAction => await DeleteControllerActionAsync(controllerAction));
        }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();

            OnControllerEventsChanged(null, null);
            ControllerProfile.ControllerEvents.CollectionChanged += OnControllerEventsChanged;
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource.Cancel();

            ControllerProfile.ControllerEvents.CollectionChanged -= OnControllerEventsChanged;
            CleanupControllerEvents();
        }

        public ControllerProfile ControllerProfile { get; }
        public ObservableCollection<ControllerEventViewModel> ControllerEvents { get; } = new ObservableCollection<ControllerEventViewModel>();

        public ICommand RenameProfileCommand { get; }
        public ICommand AddControllerEventCommand { get; }
        public ICommand ControllerActionTappedCommand { get; }
        public ICommand DeleteControllerEventCommand { get; }
        public ICommand DeleteControllerActionCommand { get; }

        private async Task RenameControllerProfileAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Translate("Rename"),
                    Translate("EnterProfileName"),
                    ControllerProfile.Name,
                    Translate("ProfileName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    _disappearingTokenSource.Token);
                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("ProfileNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);
                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.RenameControllerProfileAsync(ControllerProfile, result.Result),
                        Translate("Renaming"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task AddControllerEventAsync()
        {
            try
            {
                if (_deviceManager.Devices?.Count == 0)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ScanForDevicesFirst"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);
                    return;
                }

                var result = await _dialogService.ShowGameControllerEventDialogAsync(
                    Translate("Controller"),
                    Translate("PressButtonOrMoveJoy"),
                    Translate("Cancel"),
                    _disappearingTokenSource.Token);
                if (result.IsOk)
                {
                    ControllerEvent controllerEvent = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => controllerEvent = await _creationManager.AddOrGetControllerEventAsync(ControllerProfile, result.EventType, result.EventCode),
                        Translate("Creating"));

                    await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteControllerEvent")} {controllerEvent.EventCode}?",
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteControllerEventAsync(controllerEvent),
                        Translate("Deleting"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    Translate("AreYouSureToDeleteThisControllerAcrion"),
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            var controllerEvent = controllerAction.ControllerEvent;
                            await _creationManager.DeleteControllerActionAsync(controllerAction);
                            if (controllerEvent.ControllerActions.Count == 0)
                            {
                                await _creationManager.DeleteControllerEventAsync(controllerEvent);
                            }
                        },
                        Translate("Deleting"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnControllerEventsChanged(object sender, EventArgs args)
        {
            CleanupControllerEvents();
            foreach (var controllerEvent in ControllerProfile.ControllerEvents)
            {
                ControllerEvents.Add(new ControllerEventViewModel(controllerEvent, _deviceManager, TranslationService));
            }
        }

        private void CleanupControllerEvents()
        {
            foreach (var controllerEventViewModel in ControllerEvents)
            {
                controllerEventViewModel.Dispose();
            }

            ControllerEvents.Clear();
        }

        public class ControllerActionViewModel
        {
            private readonly ITranslationService _translationService;

            public ControllerActionViewModel(ControllerAction controllerAction, IDeviceManager deviceManager, ITranslationService translationService)
            {
                _translationService = translationService;

                ControllerAction = controllerAction;
                var device = deviceManager.GetDeviceById(controllerAction.DeviceId);

                DeviceMissing = device == null;
                DeviceName = device != null ? device.Name : Translate("Missing");
                ChannelName = device.GetChannelName(controllerAction.Channel, _translationService);
                InvertName = controllerAction.IsInvert ? Translate("Inv") : string.Empty;
            }

            public ControllerAction ControllerAction { get; }
            public bool DeviceMissing { get; }
            public string DeviceName { get; }
            public string ChannelName { get; }
            public string InvertName { get; }

            private string Translate(string key) => _translationService.Translate(key);
        }

        public class ControllerEventViewModel : ObservableCollection<ControllerActionViewModel>, IDisposable
        {
            private readonly IDeviceManager _deviceManager;
            private readonly ITranslationService _translationService;

            public ControllerEventViewModel(ControllerEvent controllerEvent, IDeviceManager deviceManager, ITranslationService translationService)
            {
                ControllerEvent = controllerEvent;
                _deviceManager = deviceManager;
                _translationService = translationService;

                PopulateGroup(controllerEvent, deviceManager, translationService);
                controllerEvent.ControllerActions.CollectionChanged += OnCollectionChanged;
            }

            public ControllerEvent ControllerEvent { get; }

            public void Dispose()
            {
                ControllerEvent.ControllerActions.CollectionChanged -= OnCollectionChanged;
                Clear();
            }

            private void OnCollectionChanged(object sender, EventArgs args)
            {
                PopulateGroup(ControllerEvent, _deviceManager, _translationService);
            }

            private void PopulateGroup(ControllerEvent controllerEvent, IDeviceManager deviceManager, ITranslationService translationService)
            {
                Clear();
                foreach (var controllerAction in controllerEvent.ControllerActions)
                {
                    Add(new ControllerActionViewModel(controllerAction, deviceManager, translationService));
                }
            }
        }
    }
}
