using System.Threading.Tasks;
using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;
using System.Windows.Input;
using Xamarin.Forms;
using BrickController2.UI.Services;

namespace BrickController2.UI.ViewModels
{
    public class CreationDetailsPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;

        public CreationDetailsPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;

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
            var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new creation name", Creation.Name, "Creation name", "Rename", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Creation name can not be empty.", "Ok");
                    return;
                }

                using (_dialogService.ShowProgressDialog(false, "Renaming..."))
                {
                    await _creationManager.RenameCreationAsync(Creation, result.Result);
                }
            }
        }

        private async Task DeleteCreation()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this creation?", "Yes", "No"))
            {
                using (_dialogService.ShowProgressDialog(false, "Deleting..."))
                {
                    await _creationManager.DeleteCreationAsync(Creation);
                }

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task AddControllerProfile()
        {
            var result = await _dialogService.ShowInputDialogAsync("Controller profile", "Enter a profile name", null, "Profile name", "Create", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Controller profile name can not be empty.", "Ok");
                    return;
                }

                ControllerProfile controllerProfile;
                using (_dialogService.ShowProgressDialog(false, "Creating..."))
                {
                    controllerProfile = await _creationManager.AddControllerProfileAsync(Creation, result.Result);
                }

                await NavigationService.NavigateToAsync<ControllerProfileDetailsPageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile)));
            }
        }
    }
}
