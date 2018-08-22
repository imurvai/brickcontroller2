using Acr.UserDialogs;
using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IUserDialogs _userDialogs;

        public DeviceListPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IUserDialogs userDialogs) 
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _userDialogs = userDialogs;

            ScanCommand = new Command(async () =>
            {
                var tokenSource = new CancellationTokenSource();
                var progressConfig = new ProgressDialogConfig()
                    .SetTitle("Scanning...")
                    .SetIsDeterministic(true)
                    .SetCancel("Cancel", () => tokenSource.Cancel());

                var percent = 0;
                using (var progressDialog = _userDialogs.Progress(progressConfig))
                {
                    var scanTask = _deviceManager.ScanAsync(tokenSource.Token);

                    while (!tokenSource.Token.IsCancellationRequested && percent <= 100)
                    {
                        progressDialog.PercentComplete = percent;
                        await Task.Delay(100);
                        percent += 1;
                    }

                    tokenSource.Cancel();
                    await scanTask;
                }
            });

            DeviceTappedCommand = new Command(async device =>
            {
                await NavigationService.NavigateToAsync<DeviceDetailsPageViewModel>(new NavigationParameters(("device", device)));
            });
        }

        public ObservableCollection<DeviceManagement.Device> Devices => _deviceManager.Devices;

        public ICommand ScanCommand { get; }
        public ICommand DeviceTappedCommand { get; }
    }
}
