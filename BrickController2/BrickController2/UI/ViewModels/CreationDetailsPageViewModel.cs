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

            EditCreationCommand = new Command(async () =>
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
            });

            DeleteCreationCommand = new Command(async () =>
            {
                if (await _userDialogs.ConfirmAsync("Are you sure to delete this creation?", "Question", "Yes", "No"))
                {
                    var progressConfig = new ProgressDialogConfig()
                        .SetIsDeterministic(false)
                        .SetTitle("Renaming...");

                    using (_userDialogs.Progress(progressConfig))
                    {
                        await _creationManager.DeleteCreationAsync(Creation);
                    }

                    await NavigationService.NavigateBackAsync();
                }
            });

            AddControllerProfileCommand = new Command(async () =>
            {

            });

            ControllerProfileTappedCommand = new Command(async controllerProfile =>
            {

            });
        }

        public Creation Creation { get; }

        public ICommand EditCreationCommand { get; }
        public ICommand DeleteCreationCommand { get; }
        public ICommand AddControllerProfileCommand { get; }
        public ICommand ControllerProfileTappedCommand { get; }
    }
}
