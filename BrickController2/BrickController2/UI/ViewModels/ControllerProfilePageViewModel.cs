using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfilePageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;

        public ControllerProfilePageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");

            RenameProfileCommand = new SafeCommand(async () => await RenameControllerProfileAsync());
            DeleteProfileCommand = new SafeCommand(async () => await DeleteControllerProfileAsync());
            AddControllerEventCommand = new SafeCommand(async () => await AddControllerEventAsync());
            ControllerEventTappedCommand = new SafeCommand<ControllerEvent>(async controllerEvent => await NavigationService.NavigateToAsync<ControllerEventPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent))));
        }

        public ControllerProfile ControllerProfile { get; }

        public ICommand RenameProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }
        public ICommand AddControllerEventCommand { get; }
        public ICommand ControllerEventTappedCommand { get; }

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
            //var result = await _dialogService.ShowGameControllerEventDialogAsync("Controller", "Press a button or move a joy on the game controller", "Cancel");

            var result = new GameControllerEventDialogResult(true, HardwareServices.GameController.GameControllerEventType.Button, "ButtonA");

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
    }
}
