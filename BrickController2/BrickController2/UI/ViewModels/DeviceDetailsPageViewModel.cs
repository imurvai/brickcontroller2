using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using BrickController2.UI.Services;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class DeviceDetailsPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;

        public DeviceDetailsPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            Device = parameters.Get<Device>("device");

            RenameDeviceCommand = new Command(async () =>
            {
                var result = await dialogService.ShowInputDialogAsync("Rename", "Enter a new name for the device", Device.Name, "Device name", "Rename", "Cancel");
                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await DisplayAlertAsync("Warning", "Device name can not be empty.", "Ok");
                        return;
                    }

                    using (dialogService.ShowProgressDialog(false, "Renaming..."))
                    {
                        await _deviceManager.RenameDeviceAsync(Device, result.Result);
                    }
                }
            });
        }

        public Device Device { get; }

        public ICommand RenameDeviceCommand { get; }
    }
}
