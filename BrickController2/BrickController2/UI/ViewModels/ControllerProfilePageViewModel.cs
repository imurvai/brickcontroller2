using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using BrickController2.DeviceManagement;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfilePageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;

        public ControllerProfilePageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            DeviceManager = deviceManager;
            _dialogService = dialogService;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");

            RenameProfileCommand = new SafeCommand(async () => await RenameControllerProfileAsync());
            DeleteProfileCommand = new SafeCommand(async () => await DeleteControllerProfileAsync());
            AddControllerEventCommand = new SafeCommand(async () => await AddControllerEventAsync());
            ControllerEventTappedCommand = new SafeCommand<ControllerEvent>(async controllerEvent => await NavigationService.NavigateToAsync<ControllerEventPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent))));
            ControllerActionTappedCommand = new SafeCommand<ControllerAction>(async controllerAction => await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controlleraction", controllerAction))));
            DeleteControllerEventCommand = new SafeCommand<ControllerEvent>(async controllerEvent => await DeleteControllerEventAsync(controllerEvent));
            DeleteControllerActionCommand = new SafeCommand<ControllerAction>(async controllerAction => await DeleteControllerActionAsync(controllerAction));
        }

        public IDeviceManager DeviceManager { get; }
        public ControllerProfile ControllerProfile { get; }

        public ICommand RenameProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }
        public ICommand AddControllerEventCommand { get; }
        public ICommand ControllerEventTappedCommand { get; }
        public ICommand ControllerActionTappedCommand { get; }
        public ICommand DeleteControllerEventCommand { get; }
        public ICommand DeleteControllerActionCommand { get; }

        private async Task RenameControllerProfileAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new profile name", ControllerProfile.Name, "Profile name", "Rename", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Profile name can not be empty.", "Ok");
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.RenameControllerProfileAsync(ControllerProfile, result.Result),
                    "Renaming...");
            }
        }

        private async Task DeleteControllerProfileAsync()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this profile?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.DeleteControllerProfileAsync(ControllerProfile),
                    "Deleting...");

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task AddControllerEventAsync()
        {
            var result = await _dialogService.ShowGameControllerEventDialogAsync("Controller", "Press a button or move a joy on the game controller", "Cancel");
            if (result.IsOk)
            {
                ControllerEvent controllerEvent = null;
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => controllerEvent = await _creationManager.AddOrGetControllerEventAsync(ControllerProfile, result.EventType, result.EventCode),
                    "Creating...");

                await NavigationService.NavigateToAsync<ControllerEventPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent)));
            }
        }

        private async Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", $"Are you sure to delete controller event {controllerEvent.EventCode}?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.DeleteControllerEventAsync(controllerEvent),
                    "Deleting...");
            }
        }

        private async Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete controller action?", "Yes", "No"))
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

        public class ControllerActionViewModel
        {
            public ControllerActionViewModel(ControllerAction controllerAction, IDeviceManager deviceManager)
            {
                ControllerAction = controllerAction;
                var device = deviceManager.GetDeviceById(controllerAction.DeviceId);
                DeviceName = device != null ? device.Name : "Missing";
                ChannelName = (device == null || device.DeviceType != DeviceType.Infrared) ? $"{controllerAction.Channel + 1}" : (controllerAction.Channel == 0 ? "Blue" : "Red");
                InvertName = controllerAction.IsInvert ? "Inv" : string.Empty;
            }

            public ControllerAction ControllerAction { get; }
            public string DeviceName { get; }
            public string ChannelName { get; }
            public string InvertName { get; }
        }

        public class ControllerActionGroupViewModel : ObservableCollection<ControllerActionViewModel>
        {
            public ControllerActionGroupViewModel(ControllerEvent controllerEvent, IDeviceManager deviceManager)
            {
                // TODO...
            }
        }
    }
}
