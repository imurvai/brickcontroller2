using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        public DeviceListPageViewModel(INavigationService navigationService, IDeviceManager deviceManager) 
            : base(navigationService)
        {
            DeviceManager = deviceManager;

            ScanCommand = new Command(async () =>
            {
                var tokenSource = new CancellationTokenSource();
                var scanTask = DeviceManager.ScanAsync(tokenSource.Token);
                await DisplayAlertAsync(null, "Scanning...", "Cancel");
                tokenSource.Cancel();
                await scanTask;
            });
        }

        public IDeviceManager DeviceManager { get; }

        public ICommand ScanCommand { get; }
    }
}
