using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;
using DeviceType = BrickController2.DeviceManagement.DeviceType;

namespace BrickController2.UI.ViewModels
{
    public class PlayerPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IGameControllerService _gameControllerService;
        private readonly IPlayLogic _playLogic;

        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<Device> _buwizzDevices = new List<Device>();
        private readonly IList<Device> _buwizz2Devices = new List<Device>();

        private Task _connectionTask;
        private CancellationTokenSource _connectionTokenSource;
        private bool _isDisappearing = false;
        private CancellationTokenSource _disappearingTokenSource;

        public PlayerPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IGameControllerService gameControllerService,
            IPlayLogic playLogic,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _gameControllerService = gameControllerService;
            _playLogic = playLogic;

            Creation = parameters.Get<Creation>("creation");
            ActiveProfile = Creation.ControllerProfiles.First();

            CollectDevices();

            BuWizzOutputLevelChangedCommand = new SafeCommand<int>(level => ChangeOutputLevel(level, _buwizzDevices));
            BuWizz2OutputLevelChangedCommand = new SafeCommand<int>(level => ChangeOutputLevel(level, _buwizz2Devices));
        }

        public Creation Creation { get; }
        public ControllerProfile ActiveProfile
        {
            get => _playLogic.ActiveProfile;
            set => _playLogic.ActiveProfile = value;
        }

        public bool HasBuWizzDevice => _buwizzDevices.Count > 0;
        public bool HasBuWizz2Device => _buwizz2Devices.Count > 0;

        public ICommand BuWizzOutputLevelChangedCommand { get; }
        public ICommand BuWizz2OutputLevelChangedCommand { get; }

        public int BuWizzOutputLevel { get; set; } = 1;
        public int BuWizz2OutputLevel { get; set; } = 1;

        public override async void OnAppearing()
        {
            _isDisappearing = false;
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();

            if (_devices.Any(d => d.DeviceType != DeviceType.Infrared) && !_deviceManager.IsBluetoothOn)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("TurnOnBluetoothToConnectBluetoothDevices"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);

                await NavigationService.NavigateBackAsync();
                return;
            }

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;

            _connectionTokenSource = new CancellationTokenSource();
            _connectionTask = ConnectDevicesAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;

            _gameControllerService.GameControllerEvent -= GameControllerEventHandler;

            _playLogic.StopPlay();

            if (_connectionTokenSource != null && _connectionTask != null)
            {
                _connectionTokenSource.Cancel();
                await _connectionTask;
            }

            await DisconnectDevicesAsync();
        }

        private void CollectDevices()
        {
            var deviceIds = Creation.GetDeviceIds();
            foreach (var deviceId in deviceIds)
            {
                var device = _deviceManager.GetDeviceById(deviceId);
                if (device != null && !_devices.Contains(device))
                {
                    _devices.Add(device);

                    if (device.DeviceType == DeviceType.BuWizz)
                    {
                        _buwizzDevices.Add(device);
                    }

                    if (device.DeviceType == DeviceType.BuWizz2)
                    {
                        _buwizz2Devices.Add(device);
                    }
                }
            }
        }

        private async Task ConnectDevicesAsync()
        {
            while (!_connectionTokenSource.IsCancellationRequested)
            {
                var deviceToConnectTo = GetNextDeviceToConnectTo();
                if (deviceToConnectTo != null)
                {
                    _playLogic.StopPlay();

                    var connectionResult = DeviceConnectionResult.Ok;

                    var dialogResult = await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            do
                            {
                                progressDialog.Message = deviceToConnectTo.Name;

                                var channelConfigs = Creation.ControllerProfiles
                                    .SelectMany(cp => cp.ControllerEvents.SelectMany(ce => ce.ControllerActions))
                                    .Where(ca => ca.DeviceId == deviceToConnectTo.Id)
                                    .Select(ca => new ChannelConfiguration
                                    {
                                        Channel = ca.Channel,
                                        ChannelOutputType = ca.ChannelOutputType,
                                        MaxServoAngle = ca.MaxServoAngle,
                                        ServoBaseAngle = ca.ServoBaseAngle,
                                        StepperAngle = ca.StepperAngle
                                    });

                                await deviceToConnectTo.ConnectAsync(
                                    false,
                                    OnDeviceDisconnected,
                                    channelConfigs,
                                    true,
                                    false,
                                    token);

                                if (token.IsCancellationRequested)
                                {
                                    return;
                                }

                                deviceToConnectTo = GetNextDeviceToConnectTo();
                            }
                            while (deviceToConnectTo != null);
                        },
                        Translate("ConnectingTo"),
                        deviceToConnectTo.Name,
                        Translate("Cancel"),
                        _connectionTokenSource.Token);

                    if (dialogResult.IsCancelled)
                    {
                        await DisconnectDevicesAsync();

                        if (!_isDisappearing)
                        {
                            await NavigationService.NavigateBackAsync();
                            return;
                        }
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
                            ChangeOutputLevel(BuWizzOutputLevel, _buwizzDevices);
                            ChangeOutputLevel(BuWizz2OutputLevel, _buwizz2Devices);

                            _playLogic.StartPlay();
                        }
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private Device GetNextDeviceToConnectTo()
        {
            Device deviceToConnectTo = null;

            foreach (var device in _devices)
            {
                if (device.DeviceState != DeviceState.Connected)
                {
                    if (device.CanBePowerSource)
                    {
                        return device;
                    }
                    else
                    {
                        deviceToConnectTo = device;
                    }
                }
            }

            return deviceToConnectTo;
        }

        private async Task DisconnectDevicesAsync()
        {
            var tasks = new List<Task>();

            foreach (var device in _devices)
            {
                tasks.Add(device.DisconnectAsync());
            }

            await Task.WhenAll(tasks);
        }

        private void OnDeviceDisconnected(Device device)
        {
        }

        private void ChangeOutputLevel(int level, IList<Device> devices)
        {
            foreach (var device in devices)
            {
                device.SetOutputLevel(level);
            }
        }

        private void GameControllerEventHandler(object sender, GameControllerEventArgs e)
        {
            _playLogic?.ProcessGameControllerEvent(e);
        }
    }
}
