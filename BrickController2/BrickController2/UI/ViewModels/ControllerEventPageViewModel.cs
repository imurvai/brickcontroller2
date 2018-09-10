using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
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
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public ControllerEventPageViewModel(
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

            ControllerEvent = parameters.Get<ControllerEvent>("controllerevent");

            DeleteControllerEventCommand = new SafeCommand(async () => await DeleteControllerEventAsync());
            AddControllerActionCommand = new SafeCommand(async () => await AddControllerActionAsync());
            ControllerActionTappedCommand = new SafeCommand<ControllerAction>(async controllerAction => await ControllerActionTapped(controllerAction));
        }

        public ControllerEvent ControllerEvent { get; }

        public ICommand DeleteControllerEventCommand { get; }
        public ICommand AddControllerActionCommand { get; }
        public ICommand ControllerActionTappedCommand { get; }

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
            if (_deviceManager.Devices.Count == 0)
            {
                await _dialogService.ShowMessageBoxAsync("Warning", "Scan for devices before adding controller actions.", "Ok");
                return;
            }

            await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controllerevent", ControllerEvent)));
        }

        private async Task ControllerActionTapped(ControllerAction controllerAction)
        {
            await NavigationService.NavigateToAsync<ControllerActionPageViewModel>(new NavigationParameters(("controllerevent", ControllerEvent), ("controlleraction", controllerAction)));
        }
    }
}
