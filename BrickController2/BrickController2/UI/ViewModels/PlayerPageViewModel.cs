using BrickController2.BusinessLogic;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.Services.MainThread;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class PlayerPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IGameControllerService _gameControllerService;
        private readonly IMainThreadService _uIThreadService;
        private readonly IPlayLogic _playLogic;

        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<Device> _buwizzDevices = new List<Device>();
        private readonly IList<Device> _buwizz2Devices = new List<Device>();

        private readonly IDictionary<Device, Task<DeviceConnectionResult>> _deviceConnectionTasks = new Dictionary<Device, Task<DeviceConnectionResult>>();
        private Task _connectionTask;
        private CancellationTokenSource _connectionTokenSource;
        private TaskCompletionSource<bool> _connectionCompletionSource;
        private bool _reconnect = false;
        private bool _isDisappearing = false;
        private CancellationTokenSource _disappearingTokenSource;

        public PlayerPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IGameControllerService gameControllerService,
            IMainThreadService uIThreadService,
            IPlayLogic playLogic,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _gameControllerService = gameControllerService;
            _uIThreadService = uIThreadService;
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

            _playLogic.StartPlay();
            _connectionTask = ConnectDevicesAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;

            _gameControllerService.GameControllerEvent -= GameControllerEventHandler;

            _playLogic.StopPlay();
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

        private void StartDeviceConnectTasks(IList<Device> devices)
        {
            foreach (var device in devices)
            {
                if (device.DeviceState == DeviceState.Disconnected && !_deviceConnectionTasks.ContainsKey(device))
                {
                    var channelConfigs = Creation.ControllerProfiles
                        .SelectMany(cp => cp.ControllerEvents.SelectMany(ce => ce.ControllerActions))
                        .Where(ca => ca.DeviceId == device.Id)
                        .Select(ca => new ChannelConfiguration
                        {
                            Channel = ca.Channel,
                            ChannelOutputType = ca.ChannelOutputType,
                            MaxServoAngle = ca.MaxServoAngle,
                            ServoBaseAngle = ca.ServoBaseAngle,
                            StepperAngle = ca.StepperAngle
                        });

                    _deviceConnectionTasks[device] = device.ConnectAsync(
                        _reconnect,
                        OnDeviceDisconnected,
                        channelConfigs,
                        true,
                        false,
                        _connectionTokenSource.Token);
                }
            }
        }
        private async Task ConnectDevicesAsync()
        {
            bool showProgress = true;

            if (_connectionTokenSource == null)
            {
                _connectionTokenSource = new CancellationTokenSource();
                _connectionCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                showProgress = true;
            }

            if (!showProgress)
            {
                return;
            }

            await _dialogService.ShowProgressDialogAsync(
                true,
                async (progressDialog, token) =>
                {
                    using (token.Register(() => _connectionTokenSource?.Cancel()))
                    {
                        // Connect devices that might be powering other devices first
                        StartDeviceConnectTasks(_buwizzDevices.Concat(_buwizz2Devices).ToList());
                        do
                        {
                            while (_deviceConnectionTasks.Values.Any(t => !t.IsCompleted))
                            {
                                int connected = _devices.Where(d => d.DeviceState == DeviceState.Connected).Count();
                                progressDialog.Message = string.Format(Translate("NofMDevicesConnected"), connected, _devices.Count());
                                progressDialog.Percent = (int)(100F * connected / _devices.Count());
                                await Task.WhenAny(_deviceConnectionTasks.Values.Where(t => !t.IsCompleted));
                            }
                            // Now attempt connection of any remaining devices
                            StartDeviceConnectTasks(_devices);
                        }
                        while (_deviceConnectionTasks.Values.Any(t => !t.IsCompleted));

                    }
                },
                Translate("Connecting"),
                string.Format(Translate("NofMDevicesConnected"), 0, _devices.Count()),
                Translate("Cancel"));

            _connectionTokenSource.Dispose();
            _connectionTokenSource = null;
            _connectionCompletionSource.TrySetResult(true);
            _deviceConnectionTasks.Clear();

            if (_devices.All(d => d.DeviceState == DeviceState.Connected))
            {
                _reconnect = true;
                ChangeOutputLevel(BuWizzOutputLevel, _buwizzDevices);
                ChangeOutputLevel(BuWizz2OutputLevel, _buwizz2Devices);
            }
            else
            {
                await DisconnectDevicesAsync();

                if (!_isDisappearing)
                {
                    await NavigationService.NavigateBackAsync();
                }
            }
        }

        private async Task DisconnectDevicesAsync()
        {
            if (_connectionTokenSource != null)
            {
                _connectionTokenSource.Cancel();
                await _connectionCompletionSource.Task;
            }

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    var tasks = new List<Task>();

                    foreach (var device in _devices)
                    {
                        tasks.Add(device.DisconnectAsync());
                    }

                    await Task.WhenAll(tasks);
                },
                Translate("Disconnecting"),
                null,
                null);
        }

        private void OnDeviceDisconnected(Device device)
        {
            _uIThreadService.RunOnMainThread(() =>
            {
                _connectionTask = ConnectDevicesAsync();
            });
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
