using System;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        public DeviceListPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService) 
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            ScanCommand = new Command(async () => await ScanAsync());
            DeviceTappedCommand = new Command<Device>(async device => await NavigationService.NavigateToAsync<DeviceDetailsPageViewModel>(new NavigationParameters(("device", device))));
        }

        public ObservableCollection<Device> Devices => _deviceManager.Devices;

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
                        var scanTask = _deviceManager.ScanAsync(cts.Token);

                        try
                        {
                            while (!token.IsCancellationRequested && percent <= 100)
                            {
                                progressDialog.Percent = percent;
                                await Task.Delay(100, token);
                                percent += 1;
                            }
                        }
                        catch (OperationCanceledException)
                        { }

                        cts.Cancel();
                        await scanTask;
                    }
                },
                "Scanning...",
                "Searching for devices.",
                "Cancel");
        }
    }
}
