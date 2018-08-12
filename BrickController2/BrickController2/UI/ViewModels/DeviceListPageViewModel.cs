using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using System.Diagnostics;
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

            StartScanCommand = new Command(async () =>
            {
                Debug.WriteLine("Start scanning...");
                var tokenSource = new CancellationTokenSource();
                var scanTask = _deviceManager.ScanAsync(tokenSource.Token);
                Debug.WriteLine("Scannig started.");
                Debug.WriteLine("Wainging for cancel...");
                await DisplayAlertAsync(null, "Scanning...", "Cancel");
                Debug.WriteLine("Cancel scanning...");
                tokenSource.Cancel();
                Debug.WriteLine("Waing for scanning to finish...");
                await scanTask;
                Debug.WriteLine("Scan finished.");
            });
        }

        public ICommand StartScanCommand { get; }
    }
}
