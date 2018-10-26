using System.Threading.Tasks;
using BrickController2.CreationManagement;
using BrickController2.UI.Services.Navigation;
using System.Windows.Input;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Commands;
using System.Linq;
using BrickController2.DeviceManagement;

namespace BrickController2.UI.ViewModels
{
    public class CreationPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public CreationPageViewModel(
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

            Creation = parameters.Get<Creation>("creation");

            RenameCreationCommand = new SafeCommand(async () => await RenameCreationAsync());
            PlayCommand = new SafeCommand(async () => await PlayAsync());
            AddControllerProfileCommand = new SafeCommand(async () => await AddControllerProfileAsync());
            ControllerProfileTappedCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile))));
            DeleteControllerProfileCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await DeleteControllerProfileAsync(controllerProfile));
        }

        public Creation Creation { get; }

        public ICommand RenameCreationCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand AddControllerProfileCommand { get; }
        public ICommand ControllerProfileTappedCommand { get; }
        public ICommand DeleteControllerProfileCommand { get; }

        private async Task RenameCreationAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new creation name", Creation.Name, "Creation name", "Rename", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await _dialogService.ShowMessageBoxAsync("Warning", "Creation name can not be empty.", "Ok");
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.RenameCreationAsync(Creation, result.Result),
                    "Renaming...");
            }
        }

        private async Task PlayAsync()
        {
            string warning = null;
            var deviceIds = Creation.GetDeviceIds();
            if (deviceIds == null || deviceIds.Count() == 0)
            {
                warning = "There are no controller actions added to the creation.";
            }
            else if (deviceIds.Any(di => _deviceManager.GetDeviceById(di) == null))
            {
                warning = "There are missing devices in the creation setup.";
            }

            if (warning == null)
            {
                await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(("creation", Creation)));
            }
            else
            {
                await _dialogService.ShowMessageBoxAsync("Warning", warning, "Ok");
            }
        }

        private async Task AddControllerProfileAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Controller profile", "Enter a profile name", null, "Profile name", "Create", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await _dialogService.ShowMessageBoxAsync("Warning", "Controller profile name can not be empty.", "Ok");
                    return;
                }

                ControllerProfile controllerProfile = null;
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => controllerProfile = await _creationManager.AddControllerProfileAsync(Creation, result.Result),
                    "Creating...");

                await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile)));
            }
        }

        private async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", $"Are you sure to delete profile {controllerProfile.Name}?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.DeleteControllerProfileAsync(controllerProfile),
                    "Deleting...");
            }
        }

        private bool IsCreationPlayable()
        {
            var deviceIds = Creation.GetDeviceIds();
            return deviceIds != null && deviceIds.Count() > 0 && deviceIds.All(di => _deviceManager.GetDeviceById(di) != null);
        }
    }
}
