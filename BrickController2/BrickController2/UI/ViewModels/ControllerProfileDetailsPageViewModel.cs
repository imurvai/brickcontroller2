using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfileDetailsPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IUserDialogs _userDialogs;

        public ControllerProfileDetailsPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IUserDialogs userDialogs,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _userDialogs = userDialogs;

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
            var promptConfig = new PromptConfig()
                .SetText(ControllerProfile.Name)
                .SetMessage("Rename the profile")
                .SetMaxLength(32)
                .SetOkText("Rename")
                .SetCancelText("Cancel");

            var result = await _userDialogs.PromptAsync(promptConfig);
            if (result.Ok)
            {
                if (string.IsNullOrWhiteSpace(result.Text))
                {
                    await DisplayAlertAsync("Warning", "Profile name can not be empty.", "Ok");
                    return;
                }

                var progressConfig = new ProgressDialogConfig()
                    .SetIsDeterministic(false)
                    .SetTitle("Renaming...");

                using (_userDialogs.Progress(progressConfig))
                {
                    await _creationManager.RenameControllerProfileAsync(ControllerProfile, result.Text);
                }
            }
        }

        private async Task DeleteControllerProfile()
        {
            if (await _userDialogs.ConfirmAsync("Are you sure to delete this profile?", "Question", "Yes", "No"))
            {
                var progressConfig = new ProgressDialogConfig()
                    .SetIsDeterministic(false)
                    .SetTitle("Deleting...");

                using (_userDialogs.Progress(progressConfig))
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
