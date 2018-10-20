using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using System;
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

        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<Device> _buwizzDevices = new List<Device>();
        private readonly IList<Device> _buwizz2Devices = new List<Device>();

        private readonly IDictionary<string, float[]> _previousButtonOutputs = new Dictionary<string, float[]>();

        private readonly IList<Task<DeviceConnectionResult>> _connectionTasks = new List<Task<DeviceConnectionResult>>();
        private CancellationTokenSource _connectionTokenSource;

        public PlayerPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IGameControllerService gameControllerService,
            NavigationParameters parameters
            )
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _gameControllerService = gameControllerService;

            Creation = parameters.Get<Creation>("creation");
            CollectDevices();
            ActiveProfile = Creation.ControllerProfiles.First();

            BuWizzOutputLevelChangedCommand = new SafeCommand<int>(level => ChangeOutputLevel(level, _buwizzDevices));
            BuWizz2OutputLevelChangedCommand = new SafeCommand<int>(level => ChangeOutputLevel(level, _buwizz2Devices));
        }

        public Creation Creation { get; }
        public ControllerProfile ActiveProfile { get; set; }

        public bool HasBuWizzDevice => _buwizzDevices.Count > 0;
        public bool HasBuWizz2Device => _buwizz2Devices.Count > 0;

        public ICommand BuWizzOutputLevelChangedCommand { get; }
        public ICommand BuWizz2OutputLevelChangedCommand { get; }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            foreach(var device in _devices)
            {
                device.DeviceStateChanged += OnDeviceStateChanged;
            }

            _gameControllerService.GameControllerEvent += GameControllerEventHandler;
            await ConnectDevicesAsync();
        }

        public override async void OnDisappearing()
        {
            foreach (var device in _devices)
            {
                device.DeviceStateChanged -= OnDeviceStateChanged;
            }

            _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
            await DisconnectDevicesAsync();

            base.OnDisappearing();
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
            _connectionTokenSource = new CancellationTokenSource();
            foreach (var device in _devices)
            {
                _connectionTasks.Add(device.ConnectAsync(_connectionTokenSource.Token));
            }

            await ShowConnectionProgress();
        }

        private async Task DisconnectDevicesAsync()
        {
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
                "Disconnecting...",
                null,
                "Cancel");
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
            foreach (var gameControllerEvent in e.ControllerEvents)
            {
                foreach (var controllerEvent in ActiveProfile.ControllerEvents)
                {
                    if (gameControllerEvent.Key.EventType == controllerEvent.EventType &&
                        gameControllerEvent.Key.EventCode == controllerEvent.EventCode)
                    {
                        foreach (var controllerAction in controllerEvent.ControllerActions)
                        {
                            var device = _deviceManager.GetDeviceById(controllerAction.DeviceId);
                            var channel = controllerAction.Channel;
                            float outputValue = 0F;

                            if (gameControllerEvent.Key.EventType == GameControllerEventType.Button)
                            {
                                var isPressed = gameControllerEvent.Value > 0.5;
                                if (!ShouldProcessButtonEvent(isPressed, controllerAction))
                                {
                                    continue;
                                }

                                outputValue = ProcessButtonEvent(gameControllerEvent.Key.EventCode, isPressed, controllerAction);
                            }
                            else if (gameControllerEvent.Key.EventType == GameControllerEventType.Axis)
                            {
                                outputValue = ProcessAxisEvent(gameControllerEvent.Key.EventCode, gameControllerEvent.Value, controllerAction);
                            }

                            if (controllerAction.MaxOutputPercent < 100)
                            {
                                outputValue = (outputValue * controllerAction.MaxOutputPercent) / 100;
                            }

                            device.SetOutput(channel, controllerAction.IsInvert ? -outputValue : outputValue);
                        }
                    }
                }
            }
        }

        private async Task ShowConnectionProgress()
        {
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    token.Register(() => _connectionTokenSource?.Cancel());
                    await Task.WhenAll(_connectionTasks);
                },
                "Connecting...",
                null,
                "Cancel");

            _connectionTokenSource.Dispose();
            _connectionTokenSource = null;
            _connectionTasks.Clear();

            if (_devices.Any(d => d.DeviceState != DeviceState.Connected))
            {
                await DisconnectDevicesAsync();
                await NavigationService.NavigateBackAsync();
            }
        }

        private async void OnDeviceStateChanged(object sender, DeviceStateChangedEventArgs args)
        {
            if (sender is Device device)
            {
                if (args.IsError && args.NewState == DeviceState.Disconnected)
                {
                    var needToShowProgress = false;
                    if (_connectionTokenSource == null)
                    {
                        _connectionTokenSource = new CancellationTokenSource();
                        needToShowProgress = true;
                    }

                    _connectionTasks.Add(device.ConnectAsync(_connectionTokenSource.Token));

                    if (needToShowProgress)
                    {
                        await ShowConnectionProgress();
                    }
                }
            }
        }

        private bool ShouldProcessButtonEvent(bool isPressed, ControllerAction controllerAction)
        {
            return controllerAction.ButtonType == ControllerButtonType.Normal || isPressed;
        }

        private float ProcessButtonEvent(string gameControllerEventCode, bool isPressed, ControllerAction controllerAction)
        {
            var previousButtonOutputs = GetPreviousButtonOutputs(gameControllerEventCode);
            float currentOutput = 0;

            switch (controllerAction.ButtonType)
            {
                case ControllerButtonType.Normal:
                    currentOutput = isPressed ? 1 : 0;
                    break;

                case ControllerButtonType.SimpleToggle:
                    currentOutput = previousButtonOutputs[0] != 0 ? 0 : 1;
                    break;

                case ControllerButtonType.Alternating:
                    currentOutput = previousButtonOutputs[0] < 0 ? 1 : -1;
                    break;

                case ControllerButtonType.Circular:
                    if (previousButtonOutputs[0] < 0)
                    {
                        currentOutput = 0;
                    }
                    else if (previousButtonOutputs[0] == 0)
                    {
                        currentOutput = 1;
                    }
                    else
                    {
                        currentOutput = -1;
                    }
                    break;

                case ControllerButtonType.PingPong:
                    if (previousButtonOutputs[0] != 0)
                    {
                        currentOutput = 0;
                    }
                    else
                    {
                        currentOutput = previousButtonOutputs[1] < 0 ? 1 : -1;
                    }
                    break;
            }

            SetPreviousButtonOutput(gameControllerEventCode, currentOutput);
            return currentOutput;
        }

        private float[] GetPreviousButtonOutputs(string gameControllerEventCode)
        {
            if (_previousButtonOutputs.ContainsKey(gameControllerEventCode))
            {
                return _previousButtonOutputs[gameControllerEventCode];
            }
            else
            {
                var prevOutputs = new float[2] { 0, 0 };
                _previousButtonOutputs[gameControllerEventCode] = prevOutputs;
                return prevOutputs;
            }
        }

        private void SetPreviousButtonOutput(string gameControllerEventCode, float value)
        {
            var buttonOutputs = _previousButtonOutputs[gameControllerEventCode];
            buttonOutputs[1] = buttonOutputs[0];
            buttonOutputs[0] = value;
        }

        private float ProcessAxisEvent(string gameControllerEventCode, float axisValue, ControllerAction controllerAction)
        {
            var axisDeadZone = controllerAction.AxisDeadZonePercent / 100F;
            if (axisDeadZone > 0)
            {
                if (Math.Abs(axisValue) <= axisDeadZone)
                {
                    return 0;
                }

                if (axisValue < 0)
                {
                    axisValue = (axisValue + axisDeadZone) / (1 - axisDeadZone);
                }
                else
                {
                    axisValue = (axisValue - axisDeadZone) / (1 - axisDeadZone);
                }
            }

            if (controllerAction.AxisCharacteristic == ControllerAxisCharacteristic.Exponential)
            {
                // Cheat :)
                axisValue = axisValue * Math.Abs(axisValue);
            }
            else if (controllerAction.AxisCharacteristic == ControllerAxisCharacteristic.Logarithmic)
            {
                // Another cheat :)
                if (axisValue < 0)
                {
                    axisValue = -(float)Math.Sqrt(Math.Abs(axisValue));
                }
                else
                {
                    axisValue = (float)Math.Sqrt(Math.Abs(axisValue));
                }
            }

            return axisValue;
        }
    }
}
