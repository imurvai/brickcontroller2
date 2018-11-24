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
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
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
                var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new profile name", ControllerProfile.Name, "Profile name", "Rename", "Cancel", _disappearingTokenSource.Token);
                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync("Warning", "Profile name can not be empty.", "Ok", _disappearingTokenSource.Token);
                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.RenameControllerProfileAsync(ControllerProfile, result.Result),
                        "Renaming...");
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
                    await _dialogService.ShowMessageBoxAsync("Warning", "Please scan for devices before adding controller events!", "Ok", _disappearingTokenSource.Token);
                    return;
                }

                var result = await _dialogService.ShowGameControllerEventDialogAsync("Controller", "Press a button or move a joy on the game controller", "Cancel", _disappearingTokenSource.Token);
                if (result.IsOk)
                {
                    ControllerEvent controllerEvent = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => controllerEvent = await _creationManager.AddOrGetControllerEventAsync(ControllerProfile, result.EventType, result.EventCode),
                        "Creating...");

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
                if (await _dialogService.ShowQuestionDialogAsync("Confirm", $"Are you sure to delete controller event {controllerEvent.EventCode}?", "Yes", "No", _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteControllerEventAsync(controllerEvent),
                        "Deleting...");
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
                if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete controller action?", "Yes", "No", _disappearingTokenSource.Token))
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
                        "Deleting...");
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
                ControllerEvents.Add(new ControllerEventViewModel(controllerEvent, _deviceManager));
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
            public ControllerActionViewModel(ControllerAction controllerAction, IDeviceManager deviceManager)
            {
                ControllerAction = controllerAction;
                var device = deviceManager.GetDeviceById(controllerAction.DeviceId);
                DeviceMissing = device == null;
                DeviceName = device != null ? device.Name : "Missing";
                ChannelName = (device == null || device.DeviceType != DeviceType.Infrared) ? $"{controllerAction.Channel + 1}" : (controllerAction.Channel == 0 ? "Blue" : "Red");
                InvertName = controllerAction.IsInvert ? "Inv" : string.Empty;
            }

            public ControllerAction ControllerAction { get; }
            public bool DeviceMissing { get; }
            public string DeviceName { get; }
            public string ChannelName { get; }
            public string InvertName { get; }
        }

        public class ControllerEventViewModel : ObservableCollection<ControllerActionViewModel>, IDisposable
        {
            private readonly IDeviceManager _deviceManager;

            public ControllerEventViewModel(ControllerEvent controllerEvent, IDeviceManager deviceManager)
            {
                ControllerEvent = controllerEvent;
                _deviceManager = deviceManager;

                PopulateGroup(controllerEvent, deviceManager);
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
                PopulateGroup(ControllerEvent, _deviceManager);
            }

            private void PopulateGroup(ControllerEvent controllerEvent, IDeviceManager deviceManager)
            {
                Clear();
                foreach (var controllerAction in controllerEvent.ControllerActions)
                {
                    Add(new ControllerActionViewModel(controllerAction, deviceManager));
                }
            }
        }
    }
}
