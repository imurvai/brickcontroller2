using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;

        public DeviceListPageViewModel(INavigationService navigationService, IDeviceManager deviceManager) 
            : base(navigationService)
        {
            _deviceManager = deviceManager;

            ScanCommand = new Command(async () =>
            {
                var tokenSource = new CancellationTokenSource();
                var scanTask = _deviceManager.ScanAsync(tokenSource.Token);
                await DisplayAlertAsync(null, "Scanning...", "Cancel");
                tokenSource.Cancel();
                await scanTask;
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
