using System.Windows.Input;
using Acr.UserDialogs;
using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
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
            IUserDialogs userDialogs,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            Device = parameters.Get<Device>("device");

            RenameDeviceCommand = new Command(async () =>
            {
                var promptConfig = new PromptConfig()
                    .SetText(Device.Name)
                    .SetMessage("Rename the device")
                    .SetMaxLength(32)
                    .SetOkText("Rename")
                    .SetCancelText("Cancel");

                var result = await userDialogs.PromptAsync(promptConfig);
                if (result.Ok)
                {
                    if (string.IsNullOrWhiteSpace(result.Text))
                    {
                        await DisplayAlertAsync("Warning", "Device name can not be empty.", "Ok");
                        return;
                    }

                    var progressConfig = new ProgressDialogConfig()
                        .SetIsDeterministic(false)
                        .SetTitle("Renaming...");

                    using (userDialogs.Progress(progressConfig))
                    {
                        await _deviceManager.RenameDeviceAsync(Device, result.Text);
                    }
                }
            });
        }

        public Device Device { get; }

        public ICommand RenameDeviceCommand { get; }
    }
}
