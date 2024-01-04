using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Translation;
using Device = BrickController2.DeviceManagement.Device;
using BrickController2.Helpers;
using Xamarin.Forms;
using System.Collections.Generic;

namespace BrickController2.UI.ViewModels
{
    public class DevicePageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _connectionTokenSource;
        private Task _connectionTask;
        private bool _reconnect = false;
        private CancellationTokenSource _disappearingTokenSource;
        private bool _isDisappearing = false;

        public DevicePageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            Device = parameters.Get<Device>("device");
            DeviceOutputs =  Enumerable
                .Range(0, Device.NumberOfChannels)
                .Select(channel => new DeviceOutputViewModel(Device, channel))
                .ToArray();

            RenameCommand = new SafeCommand(async () => await RenameDeviceAsync());
            BuWizzOutputLevelChangedCommand = new SafeCommand<int>(outputLevel => SetBuWizzOutputLevel(outputLevel));
            BuWizz2OutputLevelChangedCommand = new SafeCommand<int>(outputLevel => SetBuWizzOutputLevel(outputLevel));
            ScanCommand = new SafeCommand(async () => await ScanAsync(), () => Device.CanBePowerSource && !_deviceManager.IsScanning);
        }

        public Device Device { get; }
        public bool IsBuWizzDevice => Device.DeviceType == DeviceType.BuWizz;
        public bool IsBuWizz2Device => Device.DeviceType == DeviceType.BuWizz2;
        public bool CanBePowerSource => Device.CanBePowerSource;

        public ICommand RenameCommand { get; }
        public ICommand BuWizzOutputLevelChangedCommand { get; }
        public ICommand BuWizz2OutputLevelChangedCommand { get; }
        public ICommand ScanCommand { get; }

        public int BuWizzOutputLevel { get; set; } = 1;
        public int BuWizz2OutputLevel { get; set; } = 1;

        public IEnumerable<DeviceOutputViewModel> DeviceOutputs { get; }

        public override async void OnAppearing()
        {
            _isDisappearing = false;
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();

            if (Device.DeviceType != DeviceType.Infrared)
            {
                if (!_deviceManager.IsBluetoothOn)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("TurnOnBluetoothToConnect"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);

                    await NavigationService.NavigateBackAsync();
                    return;
                }
            }

            _connectionTokenSource = new CancellationTokenSource();
            _connectionTask = ConnectAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;
            _disappearingTokenSource.Cancel();

            if (_connectionTokenSource != null && _connectionTask != null)
            {
                _connectionTokenSource.Cancel();
                await _connectionTask;
            }

            await Device.DisconnectAsync();
        }

        private async Task RenameDeviceAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Device.Name,
                    Translate("DeviceName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (deviceName) => !string.IsNullOrEmpty(deviceName),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("DeviceNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await Device.RenameDeviceAsync(Device, result.Result),
                        Translate("Renaming"),
                        token: _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ConnectAsync()
        {
            while (!_connectionTokenSource.IsCancellationRequested)
            {
                if (Device.DeviceState != DeviceState.Connected)
                {
                    var connectionResult = DeviceConnectionResult.Ok;

                    var dialogResult = await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            using (token.Register(() => _connectionTokenSource?.Cancel()))
                            {
                                connectionResult = await Device.ConnectAsync(
                                    _reconnect,
                                    OnDeviceDisconnected,
                                    Enumerable.Empty<ChannelConfiguration>(),
                                    true,
                                    true,
                                    _connectionTokenSource.Token);
                            }
                        },
                        Translate("ConnectingTo"),
                        Device.Name,
                        Translate("Cancel"),
                        _connectionTokenSource.Token);

                    if (dialogResult.IsCancelled)
                    {
                        await Device.DisconnectAsync();

                        if (!_isDisappearing)
                        {
                            await NavigationService.NavigateBackAsync();
                        }

                        return;
                    }
                    else
                    {
                        if (connectionResult == DeviceConnectionResult.Error)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Warning"),
                                Translate("FailedToConnect"),
                                Translate("Ok"),
                                _disappearingTokenSource.Token);

                            if (!_isDisappearing)
                            {
                                await NavigationService.NavigateBackAsync();
                            }

                            return;
                        }
                        else
                        {
                            if (Device.DeviceType == DeviceType.BuWizz)
                            {
                                SetBuWizzOutputLevel(BuWizzOutputLevel);
                            }
                            else if (Device.DeviceType == DeviceType.BuWizz2)
                            {
                                SetBuWizzOutputLevel(BuWizz2OutputLevel);
                            }
                        }
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private async Task ScanAsync()
        {
            if (!_deviceManager.IsBluetoothOn)
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
                                scanTask = _deviceManager.ScanAsync(cts.Token);

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

        private void OnDeviceDisconnected(Device device)
        {
        }

        private void SetBuWizzOutputLevel(int level)
        {
            Device.SetOutputLevel(level);
        }

        public class DeviceOutputViewModel : NotifyPropertyChangedSource
        {
            private int _output;

            public DeviceOutputViewModel(Device device, int channel)
            {
                Device = device;
                Channel = channel;
                Output = 0;

                TouchUpCommand = new Command(() => Output = 0);
            }

            public Device Device { get; }
            public int Channel { get; }

            public int MinValue => -100;
            public int MaxValue => 100;

            public int Output
            {
                get { return _output; }
                set
                {
                    _output = value;
                    Device.SetOutput(Channel, (float)value / MaxValue);
                    RaisePropertyChanged();
                }
            }

            public ICommand TouchUpCommand { get; }
        }
    }
}
