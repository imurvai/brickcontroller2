using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfileDetailsPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;

        public ControllerProfileDetailsPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");

            MenuCommand = new SafeCommand(async () => await SelectMenuItem());
            AddControllerEventCommand = new SafeCommand(async () => await AddControllerEvent());
            ControllerEventTappedCommand = new SafeCommand<ControllerEvent>(async controllerEvent => await DisplayAlertAsync(controllerEvent.EventCode, "Navigation will be here.", "Ok"));
        }

        public ControllerProfile ControllerProfile { get; }

        public ICommand MenuCommand { get; }
        public ICommand AddControllerEventCommand { get; }
        public ICommand ControllerEventTappedCommand { get; }

        private async Task SelectMenuItem()
        {
            var result = await DisplayActionSheetAsync("Select an option", "Cancel", "Delete profile", "Rename profile");
            switch (result)
            {
                case "Delete profile":
                    await DeleteControllerProfile();
                    break;

                case "Rename profile":
                    await RenameControllerProfile();
                    break;
            }
        }

        private async Task RenameControllerProfile()
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

        private async Task DeleteControllerProfile()
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

        private async Task AddControllerEvent()
        {
            var result = await _dialogService.ShowGameControllerEventDialogAsync("Controller", "Press a button or move a joy on the game controller", "Cancel");
            if (result.IsOk)
            {
                ControllerEvent controllerEvent = null;
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => controllerEvent = await _creationManager.AddOrGetControllerEventAsync(ControllerProfile, result.EventType, result.EventCode),
                    "Creating...");

                //await NavigationService.NavigateToAsync<ControllerEventDetailsPageViewModel>(new NavigationParameters(("controllerevent", controllerEvent)));
            }
        }
    }
}
