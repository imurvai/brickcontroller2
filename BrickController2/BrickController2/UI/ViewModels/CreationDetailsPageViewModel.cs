using System.Threading.Tasks;
using Acr.UserDialogs;
using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class CreationDetailsPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IUserDialogs _userDialogs;

        public CreationDetailsPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IUserDialogs userDialogs,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _userDialogs = userDialogs;

            Creation = parameters.Get<Creation>("creation");

            MenuCommand = new Command(async () => await SelectMenuItem());
            AddControllerProfileCommand = new Command(async () => await AddControllerProfile());
            ControllerProfileTappedCommand = new Command<ControllerProfile>(async controllerProfile => await NavigationService.NavigateToAsync<ControllerProfileDetailsPageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile))));
        }

        public Creation Creation { get; }

        public ICommand MenuCommand { get; }
        public ICommand AddControllerProfileCommand { get; }
        public ICommand ControllerProfileTappedCommand { get; }

        private async Task SelectMenuItem()
        {
            var result = await DisplayActionSheetAsync("Select an option", "Cancel", "Delete creation", "Rename creation");
            switch (result)
            {
                case "Delete creation":
                    await DeleteCreation();
                    break;

                case "Rename creation":
                    await RenameCreation();
                    break;
            }
        }

        private async Task RenameCreation()
        {
            var promptConfig = new PromptConfig()
                .SetText(Creation.Name)
                .SetMessage("Rename the creation")
                .SetMaxLength(32)
                .SetOkText("Rename")
                .SetCancelText("Cancel");

            var result = await _userDialogs.PromptAsync(promptConfig);
            if (result.Ok)
            {
                if (string.IsNullOrWhiteSpace(result.Text))
                {
                    await DisplayAlertAsync("Warning", "Creation name can not be empty.", "Ok");
                    return;
                }

                var progressConfig = new ProgressDialogConfig()
                    .SetIsDeterministic(false)
                    .SetTitle("Renaming...");

                using (_userDialogs.Progress(progressConfig))
                {
                    await _creationManager.RenameCreationAsync(Creation, result.Text);
                }
            }
        }

        private async Task DeleteCreation()
        {
            if (await _userDialogs.ConfirmAsync("Are you sure to delete this creation?", "Question", "Yes", "No"))
            {
                var progressConfig = new ProgressDialogConfig()
                    .SetIsDeterministic(false)
                    .SetTitle("Deleting...");

                using (_userDialogs.Progress(progressConfig))
                {
                    await _creationManager.DeleteCreationAsync(Creation);
                }

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task AddControllerProfile()
        {
            var result = await _userDialogs.PromptAsync("New profile name", null, "Create", "Cancel", "Name", InputType.Default, null);
            if (result.Ok)
            {
                if (string.IsNullOrWhiteSpace(result.Text))
                {
                    await DisplayAlertAsync("Warning", "Controller profile name can not be empty.", "Ok");
                    return;
                }

                var progressConfig = new ProgressDialogConfig()
                    .SetIsDeterministic(false)
                    .SetTitle("Creating...");

                ControllerProfile controllerProfile;
                using (_userDialogs.Progress(progressConfig))
                {
                    controllerProfile = await _creationManager.AddControllerProfileAsync(Creation, result.Text);
                }

                await NavigationService.NavigateToAsync<ControllerProfileDetailsPageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile)));
            }
        }
    }
}
