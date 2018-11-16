using System;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using System.Threading.Tasks;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;
using BrickController2.UI.Commands;
using System.Threading;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDialogService _dialogService;

        public DeviceListPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService) 
            : base(navigationService)
        {
            DeviceManager = deviceManager;
            _dialogService = dialogService;

            ScanCommand = new SafeCommand(async () => await ScanAsync(), () => !DeviceManager.IsScanning);
            DeviceTappedCommand = new SafeCommand<Device>(async device => await NavigationService.NavigateToAsync<DevicePageViewModel>(new NavigationParameters(("device", device))));
            DeleteDeviceCommand = new SafeCommand<Device>(async device => await DeleteDeviceAsync(device));
        }

        public IDeviceManager DeviceManager { get; }

        public ICommand ScanCommand { get; }
        public ICommand DeviceTappedCommand { get; }
        public ICommand DeleteDeviceCommand { get; }

        private async Task DeleteDeviceAsync(Device device)
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", $"Are you sure to delete device {device.Name}?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await DeviceManager.DeleteDeviceAsync(device),
                    "Deleting...");
            }
        }

        private async Task ScanAsync()
        {
            if (!DeviceManager.IsBluetoothOn &&
                !await _dialogService.ShowQuestionDialogAsync("Warning", "Bluetooth is turned off. Do you want to proceed with the scanning?", "Yes", "No"))
            {
                return;
            }

            var percent = 0;
            var scanResult = true;
            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        Task<bool> scanTask = null;
                        try
                        {
                            scanTask = DeviceManager.ScanAsync(cts.Token);

                            while (!token.IsCancellationRequested && percent <= 100 && !scanTask.IsCompleted)
                            {
                                progressDialog.Percent = percent;
                                await Task.Delay(100, token);
                                percent += 1;
                            }
                        }
                        catch (Exception)
                        { }

                        cts.Cancel();

                        if (scanTask != null)
                        {
                            scanResult = await scanTask;
                        }
                    }
                },
                "Scanning...",
                "Searching for devices.",
                "Cancel");

            if (!scanResult)
            {
                await _dialogService.ShowMessageBoxAsync("Warning", "Error during scanning", "Ok");
            }
        }
    }
}
