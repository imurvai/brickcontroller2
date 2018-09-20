using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using Device = BrickController2.DeviceManagement.Device;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BrickController2.UI.Commands;

namespace BrickController2.UI.ViewModels
{
    public class DevicePageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public DevicePageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            Device = parameters.Get<Device>("device");

            RenameCommand = new SafeCommand(async () => await RenameDeviceAsync());
            DeleteCommand = new SafeCommand(async () => await DeleteDeviceAsync());
            ConnectCommand = new SafeCommand(async () => await ConnectAsync());

            for (int i = 0; i < Device.NumberOfChannels; i++)
            {
                Outputs.Add(new DeviceOutputViewModel(Device, i));
            }
        }

        public Device Device { get; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ConnectCommand { get; }

        public ObservableCollection<DeviceOutputViewModel> Outputs { get; } = new ObservableCollection<DeviceOutputViewModel>();

        public override void OnAppearing()
        {
            Device.DeviceStateChanged += DeviceStateChangedHandler;

            base.OnAppearing();
        }

        public override async void OnDisappearing()
        {
            Device.DeviceStateChanged -= DeviceStateChangedHandler;

            await Device.DisconnectAsync();

            base.OnDisappearing();
        }

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
                    async (progressDialog, token) => await Device.RenameDeviceAsync(Device, result.Result),
                    "Renaming...");
            }
        }

        private async Task DeleteDeviceAsync()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this device?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) =>
                    {
                        await Device.DisconnectAsync();
                        await _deviceManager.DeleteDeviceAsync(Device);
                    },
                    "Deleting...");

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task ConnectAsync()
        {
            DeviceConnectionResult connectionResult = DeviceConnectionResult.Ok;
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    connectionResult = await Device.ConnectAsync(token);
                },
                "Connecting...",
                null,
                "Cancel");

            if (connectionResult == DeviceConnectionResult.Error)
            {
                await DisplayAlertAsync("Warning", "Failed to connect to device.", "Ok");
            }
        }

        private async void DeviceStateChangedHandler(object sender, DeviceStateChangedEventArgs args)
        {
            if (args.OldState == DeviceState.Connected && args.NewState == DeviceState.Disconnected && args.IsError)
            {
                var result = await _dialogService.ShowQuestionDialogAsync("Device connection lost", "Do you want to reconnect?", "Yes", "No");
                if (result)
                {
                    await ConnectAsync();
                }
            }
        }
    }
}
