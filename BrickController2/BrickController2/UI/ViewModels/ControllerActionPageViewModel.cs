using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class ControllerActionPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public ControllerActionPageViewModel(
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

            ControllerAction = parameters.Get<ControllerAction>("controlleraction");
        }

        public ControllerAction ControllerAction { get; }
    }
}
