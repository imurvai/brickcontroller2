using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class DeviceDetailsPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly Device _device;

        public DeviceDetailsPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _device = parameters.Get<Device>("device");
        }

        public string DeviceName => _device.Name;
    }
}
