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

        private CancellationTokenSource _scanTokenSource;

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

        public override void OnDisappearing()
        {
            _scanTokenSource?.Cancel();
            base.OnDisappearing();
        }

        private async Task ScanAsync()
        {
            _scanTokenSource = new CancellationTokenSource();

            var percent = 0;
            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    var scanTask = _deviceManager.ScanAsync(_scanTokenSource.Token);

                    while (!token.IsCancellationRequested && percent <= 100)
                    {
                        progressDialog.Percent = percent;
                        await Task.Delay(100);
                        percent += 1;
                    }

                    _scanTokenSource.Cancel();
                    await scanTask;
                },
                "Scanning...",
                "Searching for devices.",
                "Cancel",
                _scanTokenSource);
        }
    }
}
