using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using BrickController2.UI.Services;
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

            ScanCommand = new Command(async () => await Scan());
            DeviceTappedCommand = new Command<Device>(async device => await NavigationService.NavigateToAsync<DeviceDetailsPageViewModel>(new NavigationParameters(("device", device))));
        }

        public ObservableCollection<DeviceManagement.Device> Devices => _deviceManager.Devices;

        public ICommand ScanCommand { get; }
        public ICommand DeviceTappedCommand { get; }

        public override void OnDisappearing()
        {
            _scanTokenSource?.Cancel();
            base.OnDisappearing();
        }

        private async Task Scan()
        {
            _scanTokenSource = new CancellationTokenSource();

            var percent = 0;
            using (var progress = _dialogService.ShowProgressDialog(true, "Scanning...", "Searching for device.", "Cancel", _scanTokenSource))
            {
                var scanTask = _deviceManager.ScanAsync(_scanTokenSource.Token);

                while (!_scanTokenSource.Token.IsCancellationRequested && percent <= 100)
                {
                    progress.Percent = percent;
                    await Task.Delay(100);
                    percent += 1;
                }

                _scanTokenSource.Cancel();
                await scanTask;
            }
        }
    }
}
