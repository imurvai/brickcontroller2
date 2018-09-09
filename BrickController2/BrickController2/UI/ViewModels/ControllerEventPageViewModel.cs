using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class ControllerEventPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;

        public ControllerEventPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;

            ControllerEvent = parameters.Get<ControllerEvent>("controllerevent");

            DeleteControllerEventCommand = new SafeCommand(async () => await DeleteControllerEventAsync());
            AddControllerActionCommand = new SafeCommand(async () => await AddControllerActionAsync());
        }

        public ControllerEvent ControllerEvent { get; }

        public ICommand DeleteControllerEventCommand { get; }
        public ICommand AddControllerActionCommand { get; }

        private async Task DeleteControllerEventAsync()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this controller event?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.DeleteControllerEventAsync(ControllerEvent),
                    "Deleting...");

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task AddControllerActionAsync()
        {
            await _dialogService.ShowMessageBoxAsync("!!!", "ControllerAction will be here.", "Okka");
        }
    }
}
