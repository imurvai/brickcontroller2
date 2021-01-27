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
using BrickController2.BusinessLogic;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.Helpers;
using System.IO;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfilePageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IPlayLogic _playLogic;

        private CancellationTokenSource _disappearingTokenSource;

        public ControllerProfilePageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService,
            IPlayLogic playLogic,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            SharedFileStorageService = sharedFileStorageService;
            _playLogic = playLogic;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");

            ExportControllerProfileCommand = new SafeCommand(async () => await ExportControllerProfileAsync(), () => SharedFileStorageService.IsSharedStorageAvailable);
            RenameProfileCommand = new SafeCommand(async () => await RenameControllerProfileAsync());
            AddControllerEventCommand = new SafeCommand(async () => await AddControllerEventAsync());
            PlayCommand = new SafeCommand(async () => await PlayAsync());
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

        public ISharedFileStorageService SharedFileStorageService { get; }

        public ICommand ExportControllerProfileCommand { get; }
        public ICommand RenameProfileCommand { get; }
        public ICommand AddControllerEventCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand ControllerActionTappedCommand { get; }
        public ICommand DeleteControllerEventCommand { get; }
        public ICommand DeleteControllerActionCommand { get; }

        private async Task ExportControllerProfileAsync()
        {
            try
            {
                var filename = ControllerProfile.Name;
                var done = false;

                do
                {
                    var result = await _dialogService.ShowInputDialogAsync(
                        filename,
                        Translate("ProfileName"),
                        Translate("Ok"),
                        Translate("Cancel"),
                        KeyboardType.Text,
                        fn => FileHelper.FilenameValidator(fn),
                        _disappearingTokenSource.Token);

                    if (!result.IsOk)
                    {
                        return;
                    }

                    filename = result.Result;
                    var filePath = Path.Combine(SharedFileStorageService.SharedStorageDirectory, $"{filename}.{FileHelper.ControllerProfileFileExtension}");

                    if (!File.Exists(filePath) ||
                        await _dialogService.ShowQuestionDialogAsync(
                            Translate("FileAlreadyExists"),
                            Translate("DoYouWantToOverWrite"),
                            Translate("Yes"),
                            Translate("No"),
                            _disappearingTokenSource.Token))
                    {
                        try
                        {
                            await _creationManager.ExportControllerProfileAsync(ControllerProfile, filePath);
                            done = true;

                            await _dialogService.ShowMessageBoxAsync(
                                Translate("ExportSuccessful"),
                                filePath,
                                Translate("Ok"),
                                _disappearingTokenSource.Token);
                        }
                        catch (Exception)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Error"),
                                Translate("FailedToExportControllerProfile"),
                                Translate("Ok"),
                                _disappearingTokenSource.Token);

                            return;
                        }
                    }
                }
                while (!done);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task RenameControllerProfileAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    ControllerProfile.Name,
                    Translate("ProfileName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (profileName) => !string.IsNullOrEmpty(profileName),
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
                        Translate("Renaming"),
                        token: _disappearingTokenSource.Token);
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
                        Translate("Creating"),
                        token: _disappearingTokenSource.Token);

                    await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PlayAsync()
        {
            var validationResult = _playLogic.ValidateCreation(ControllerProfile.Creation);

            string warning = null;
            switch (validationResult)
            {
                case CreationValidationResult.MissingControllerAction:
                    warning = Translate("NoControllerActions");
                    break;

                case CreationValidationResult.MissingDevice:
                    warning = Translate("MissingDevices");
                    break;

                case CreationValidationResult.MissingSequence:
                    warning = Translate("MissingSequence");
                    break;
            }

            if (validationResult == CreationValidationResult.Ok)
            {
                await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(("creation", ControllerProfile.Creation)));
            }
            else
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    warning,
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
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
                        Translate("Deleting"),
                        token: _disappearingTokenSource.Token);
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
                        Translate("Deleting"),
                        token: _disappearingTokenSource.Token);
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
                ControllerEvents.Add(new ControllerEventViewModel(controllerEvent, _deviceManager, _playLogic, TranslationService));
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

            public ControllerActionViewModel(
                ControllerAction controllerAction,
                IDeviceManager deviceManager,
                IPlayLogic playLogic,
                ITranslationService translationService)
            {
                _translationService = translationService;

                ControllerAction = controllerAction;
                var device = deviceManager.GetDeviceById(controllerAction.DeviceId);

                ControllerActionValid = playLogic.ValidateControllerAction(controllerAction);
                DeviceName = device != null ? device.Name : Translate("Missing");
                DeviceType = device != null ? device.DeviceType : DeviceType.Unknown;
                Channel = controllerAction.Channel;
                InvertName = controllerAction.IsInvert ? Translate("Inv") : string.Empty;
            }

            public ControllerAction ControllerAction { get; }
            public bool ControllerActionValid { get; }
            public string DeviceName { get; }
            public DeviceType DeviceType { get; }
            public int Channel { get; }
            public string InvertName { get; }

            private string Translate(string key) => _translationService.Translate(key);
        }

        public class ControllerEventViewModel : ObservableCollection<ControllerActionViewModel>, IDisposable
        {
            private readonly IDeviceManager _deviceManager;
            private readonly IPlayLogic _playLogic;
            private readonly ITranslationService _translationService;

            public ControllerEventViewModel(
                ControllerEvent controllerEvent,
                IDeviceManager deviceManager,
                IPlayLogic playLogic,
                ITranslationService translationService)
            {
                ControllerEvent = controllerEvent;
                _deviceManager = deviceManager;
                _playLogic = playLogic;
                _translationService = translationService;

                PopulateGroup(controllerEvent, deviceManager, playLogic, translationService);
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
                PopulateGroup(ControllerEvent, _deviceManager, _playLogic, _translationService);
            }

            private void PopulateGroup(
                ControllerEvent controllerEvent,
                IDeviceManager deviceManager,
                IPlayLogic playLogic,
                ITranslationService translationService)
            {
                Clear();
                foreach (var controllerAction in controllerEvent.ControllerActions)
                {
                    Add(new ControllerActionViewModel(controllerAction, deviceManager, playLogic, translationService));
                }
            }
        }
    }
}
