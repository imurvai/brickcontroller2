using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class DeviceListPageViewModel : PageViewModelBase
    {
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _disappearingTokenSource;
        private bool _isDisappearing = false;

        public DeviceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService)
            : base(navigationService, translationService)
        {
            DeviceManager = deviceManager;
            _dialogService = dialogService;

            ScanCommand = new SafeCommand(async () => await ScanAsync(), () => !DeviceManager.IsScanning);
            DeviceTappedCommand = new SafeCommand<Device>(async device => await NavigationService.NavigateToAsync<DevicePageViewModel>(new NavigationParameters(("device", device))));
            DeleteDeviceCommand = new SafeCommand<Device>(async device => await DeleteDeviceAsync(device));
        }

        public IDeviceManager DeviceManager { get; }

        public ICommand ScanCommand { get; }
        public ICommand DeviceTappedCommand { get; }
        public ICommand DeleteDeviceCommand { get; }

        public override void OnAppearing()
        {
            _isDisappearing = false;
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _isDisappearing = true;
            _disappearingTokenSource.Cancel();
        }

        private async Task DeleteDeviceAsync(Device device)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteDevice")} '{device.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await DeviceManager.DeleteDeviceAsync(device),
                        Translate("Deleting"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ScanAsync()
        {
            if (!DeviceManager.IsBluetoothOn)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("BluetoothIsTurnedOff"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }

            var percent = 0;
            var scanResult = true;
            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    if (!_isDisappearing)
                    {
                        using (var cts = new CancellationTokenSource())
                        using (_disappearingTokenSource.Token.Register(() => cts.Cancel()))
                        {
                            Task<bool> scanTask = null;
                            try
                            {
                                scanTask = DeviceManager.ScanAsync(cts.Token);

                                while (!token.IsCancellationRequested && percent <= 100 && !scanTask.IsCompleted)
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
                                scanResult = await scanTask;
                            }
                        }
                    }
                },
                Translate("Scanning"),
                Translate("SearchingForDevices"),
                Translate("Cancel"));

            if (!scanResult && !_isDisappearing)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("ErrorDuringScanning"),
                    Translate("Ok"),
                    CancellationToken.None);
            }
        }
    }
}
