using Acr.UserDialogs;
using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
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
        private readonly IUserDialogs _userDialogs;

        private CancellationTokenSource _scanTokenSource;

        public DeviceListPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IUserDialogs userDialogs) 
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _userDialogs = userDialogs;

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
            var progressConfig = new ProgressDialogConfig()
                .SetTitle("Scanning...")
                .SetIsDeterministic(true)
                .SetCancel("Cancel", () => _scanTokenSource.Cancel());

            var percent = 0;
            using (var progressDialog = _userDialogs.Progress(progressConfig))
            {
                var scanTask = _deviceManager.ScanAsync(_scanTokenSource.Token);

                while (!_scanTokenSource.Token.IsCancellationRequested && percent <= 100)
                {
                    progressDialog.PercentComplete = percent;
                    await Task.Delay(100);
                    percent += 1;
                }

                _scanTokenSource.Cancel();
                await scanTask;
            }
        }
    }
}
