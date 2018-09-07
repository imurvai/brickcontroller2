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
            DeviceTappedCommand = new SafeCommand<Device>(async device => await NavigationService.NavigateToAsync<DeviceDetailsPageViewModel>(new NavigationParameters(("device", device))));
        }

        public IDeviceManager DeviceManager { get; }

        public ICommand ScanCommand { get; }
        public ICommand DeviceTappedCommand { get; }

        private async Task ScanAsync()
        {
            var percent = 0;
            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        Task scanTask = null;
                        try
                        {
                            scanTask = DeviceManager.ScanAsync(cts.Token);

                            while (!token.IsCancellationRequested && percent <= 100)
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
                            await scanTask;
                        }
                    }
                },
                "Scanning...",
                "Searching for devices.",
                "Cancel");
        }
    }
}
