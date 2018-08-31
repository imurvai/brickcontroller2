using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels
{
    public class DeviceDetailsPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public DeviceDetailsPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            Device = parameters.Get<Device>("device");

            RenameDeviceCommand = new Command(async () => await RenameDeviceAsync());
        }

        public Device Device { get; }

        public ICommand RenameDeviceCommand { get; }

        private async Task RenameDeviceAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new name for the device", Device.Name, "Device name", "Rename", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Device name can not be empty.", "Ok");
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _deviceManager.RenameDeviceAsync(Device, result.Result),
                    "Renaming...");
            }
        }
    }
}
