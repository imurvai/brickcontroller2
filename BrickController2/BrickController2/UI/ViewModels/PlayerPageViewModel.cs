using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.HardwareServices.GameController;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class PlayerPageViewModel : PageViewModelBase
    {
        private ICreationManager _creationManager;
        private IDeviceManager _deviceManager;
        private IDialogService _dialogService;
        private IGameControllerService _gameControllerService;

        public PlayerPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IGameControllerService gameControllerService,
            NavigationParameters parameters
            )
            : base(navigationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _gameControllerService = gameControllerService;

            Creation = parameters.Get<Creation>("creation");
        }

        public Creation Creation { get; }
    }
}
