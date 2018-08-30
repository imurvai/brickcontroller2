using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;
using BrickController2.UI.Services;
using Xamarin.Forms;

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

            MenuCommand = new Command(async () => await SelectMenuItem());
            AddControllerEventCommand = new Command(async () => await AddControllerEvent());
            ControllerEventTappedCommand = new Command<ControllerEvent>(async controllerEvent => await DisplayAlertAsync(controllerEvent.EventCode, "Navigation will be here.", "Ok"));
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

                using (_dialogService.ShowProgressDialog(false, "Renaming..."))
                {
                    await _creationManager.RenameControllerProfileAsync(ControllerProfile, result.Result);
                }
            }
        }

        private async Task DeleteControllerProfile()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this profile?", "Yes", "No"))
            {
                using (_dialogService.ShowProgressDialog(false, "Deleting..."))
                {
                    await _creationManager.DeleteControllerProfileAsync(ControllerProfile);
                }

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task AddControllerEvent()
        {
            await DisplayAlertAsync(null, "Add will be here", "Ok");
        }
    }
}
