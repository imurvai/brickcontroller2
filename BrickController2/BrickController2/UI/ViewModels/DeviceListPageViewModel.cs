using System;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;
using BrickController2.UI.Commands;

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
            DeviceTappedCommand = new SafeCommand<Device>(async device => await NavigationService.NavigateToAsync<DeviceDetailsPageViewModel>(new NavigationParameters(("device", device))));
        }

        public IDeviceManager DeviceManager { get; }

        public ICommand ScanCommand { get; }
        public ICommand DeviceTappedCommand { get; }

        private async Task ScanAsync()
        {
            if (DeviceManager.IsScanning)
            {
                return;
            }

            var percent = 0;
            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    await DeviceManager.StartScanAsync();

                    try
                    {
                        while (!token.IsCancellationRequested && percent <= 100)
                        {
                            progressDialog.Percent = percent;
                            await Task.Delay(100, token);
                            percent += 1;
                        }
                    }
                    catch (Exception)
                    { }

                    await DeviceManager.StopScanAsync();
                },
                "Scanning...",
                "Searching for devices.",
                "Cancel");
        }
    }
}
