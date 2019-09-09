using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.Services.UIThread;
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
        private readonly IUIThreadService _uIThreadService;

        private readonly IList<Device> _devices = new List<Device>();
        private readonly IList<Device> _buwizzDevices = new List<Device>();
        private readonly IList<Device> _buwizz2Devices = new List<Device>();

        private readonly IDictionary<(string EventCode, ControllerAction ControllerAction), float[]> _previousButtonOutputs = new Dictionary<(string, ControllerAction), float[]>();
        private readonly IDictionary<(string EventCode, ControllerAction ControllerAction), float> _previousAxisOutputs = new Dictionary<(string, ControllerAction), float>();
        private readonly IDictionary<ControllerAction, bool> _disabledOutputForAxises = new Dictionary<ControllerAction, bool>();
        private readonly IDictionary<(string DeviceId, int Channel), IDictionary<(GameControllerEventType EventType, string EventCode), float>> _axisOutputValues = new Dictionary<(string, int), IDictionary<(GameControllerEventType, string), float>>();

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
            IUIThreadService uIThreadService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _gameControllerService = gameControllerService;
            _uIThreadService = uIThreadService;

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

            _connectionTask = ConnectDevicesAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;

            _gameControllerService.GameControllerEvent -= GameControllerEventHandler;

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
            bool showProgress = false;

            if (_connectionTokenSource == null)
            {
                _connectionTokenSource = new CancellationTokenSource();
                _connectionCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                showProgress = true;
            }

            foreach (var device in _devices)
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
                            ServoBaseAngle = ca.ServoBaseAngle
                        });

                    _deviceConnectionTasks[device] = device.ConnectAsync(
                        _reconnect,
                        OnDeviceDisconnected,
                        channelConfigs,
                        _connectionTokenSource.Token);
                }
            }

            if (!showProgress)
            {
                return;
            }

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    using (token.Register(() => _connectionTokenSource?.Cancel()))
                    {
                        while (_deviceConnectionTasks.Values.Any(t => !t.IsCompleted))
                        {
                            await Task.WhenAll(_deviceConnectionTasks.Values);
                        }
                    }
                },
                Translate("Connecting"),
                null,
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
                                StoreAxisOutputValue(outputValue, controllerAction.DeviceId, controllerAction.Channel, controllerEvent.EventType, controllerEvent.EventCode);
                                outputValue = CombineAxisOutputValues(controllerAction.DeviceId, controllerAction.Channel);
                            }

                            device.SetOutput(channel, outputValue);
                        }
                    }
                }
            }
        }

        private static bool ShouldProcessButtonEvent(bool isPressed, ControllerAction controllerAction)
        {
            return controllerAction.ButtonType == ControllerButtonType.Normal || isPressed;
        }

        private float ProcessButtonEvent(string gameControllerEventCode, bool isPressed, ControllerAction controllerAction)
        {
            var previousButtonOutputs = GetPreviousButtonOutputs(gameControllerEventCode, controllerAction);
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

                case ControllerButtonType.Stop:
                    currentOutput = 0;

                    SetIsOutputDisabledForAxises(controllerAction, false);
                    ResetPreviousAxisOutputsForOutput(controllerAction);

                    break;
            }

            SetPreviousButtonOutput(gameControllerEventCode, controllerAction, currentOutput);
            return AdjustOutputValue(currentOutput, controllerAction);
        }

        private float[] GetPreviousButtonOutputs(string gameControllerEventCode, ControllerAction controllerAction)
        {
            if (_previousButtonOutputs.ContainsKey((gameControllerEventCode, controllerAction)))
            {
                return _previousButtonOutputs[(gameControllerEventCode, controllerAction)];
            }
            else
            {
                var prevOutputs = new float[2] { 0, 0 };
                _previousButtonOutputs[(gameControllerEventCode, controllerAction)] = prevOutputs;
                return prevOutputs;
            }
        }

        private void SetPreviousButtonOutput(string gameControllerEventCode, ControllerAction controllerAction, float value)
        {
            var buttonOutputs = _previousButtonOutputs[(gameControllerEventCode, controllerAction)];
            buttonOutputs[1] = buttonOutputs[0];
            buttonOutputs[0] = value;
        }

        private float ProcessAxisEvent(string gameControllerEventCode, float axisValue, ControllerAction controllerAction)
        {
            var previousAxisValue = GetPreviousAxisOutput(gameControllerEventCode, controllerAction);

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

            if (controllerAction.AxisType == ControllerAxisType.Train)
            {
                if (GetIsOutputDisableForAxises(controllerAction))
                {
                    if (axisValue == 0)
                    {
                        SetIsOutputDisabledForAxises(controllerAction, false);
                    }

                    axisValue = previousAxisValue;
                }
                else
                {
                    if (axisValue * previousAxisValue >= 0)
                    {
                        // The sign of axisValue and previouAxisValue are same
                        if (Math.Abs(axisValue) < Math.Abs(previousAxisValue))
                        {
                            // Don't accelarate
                            axisValue = previousAxisValue;
                        }
                    }
                    else
                    {
                        // The sign of axisValue and previousAxisValue are different
                        if (Math.Abs(previousAxisValue - axisValue) < 1)
                        {
                            // Don't slow down
                            axisValue = previousAxisValue;
                        }
                        else
                        {
                            // Slow down
                            if (previousAxisValue > 0)
                            {
                                axisValue = previousAxisValue - (previousAxisValue - axisValue - 1);
                            }
                            else
                            {
                                axisValue = previousAxisValue - (previousAxisValue - axisValue + 1);
                            }

                            if (axisValue == 0)
                            {
                                SetIsOutputDisabledForAxises(controllerAction, true);
                            }
                        }
                    }
                }
            }

            SetPreviousAxisOutput(gameControllerEventCode, controllerAction, axisValue);
            return AdjustOutputValue(axisValue, controllerAction);
        }

        private float GetPreviousAxisOutput(string gameControllerEventCode, ControllerAction controllerAction)
        {
            if (_previousAxisOutputs.ContainsKey((gameControllerEventCode, controllerAction)))
            {
                return _previousAxisOutputs[(gameControllerEventCode, controllerAction)];
            }
            else
            {
                var prevOutput = 0.0f;
                _previousAxisOutputs[(gameControllerEventCode, controllerAction)] = prevOutput;
                return prevOutput;
            }
        }

        private void SetPreviousAxisOutput(string gameControllerEventCode, ControllerAction controllerAction, float value)
        {
            _previousAxisOutputs[(gameControllerEventCode, controllerAction)] = value;
        }

        private void StoreAxisOutputValue(float outputValue, string deviceId, int channel, GameControllerEventType controllerEventType, string controllerEventCode)
        {
            var axisOutputValuesKey = (deviceId, channel);
            if (!_axisOutputValues.ContainsKey(axisOutputValuesKey))
            {
                _axisOutputValues[axisOutputValuesKey] = new Dictionary<(GameControllerEventType, string), float>();
            }

            _axisOutputValues[axisOutputValuesKey][(controllerEventType, controllerEventCode)] = outputValue;
        }

        private bool GetIsOutputDisableForAxises(ControllerAction controllerAction)
        {
            if (!_disabledOutputForAxises.ContainsKey(controllerAction))
            {
                _disabledOutputForAxises[controllerAction] = false;
            }

            return _disabledOutputForAxises[controllerAction];
        }

        private void SetIsOutputDisabledForAxises(ControllerAction controllerAction, bool value)
        {
            _disabledOutputForAxises[controllerAction] = value;
        }

        private void ResetPreviousAxisOutputsForOutput(ControllerAction controllerAction)
        {
            foreach (var key in _previousAxisOutputs.Keys.Where(k => k.ControllerAction.DeviceId == controllerAction.DeviceId && k.ControllerAction.Channel == controllerAction.Channel).ToArray())
            {
                _previousAxisOutputs[key] = 0;
            }
        }

        private float CombineAxisOutputValues(string deviceId, int channel)
        {
            var axisOutputValuesKey = (deviceId, channel);
            if (!_axisOutputValues.ContainsKey(axisOutputValuesKey))
            {
                return 0.0F;
            }

            var result = 0.0F;
            foreach (var outputValue in _axisOutputValues[axisOutputValuesKey].Values)
            {
                result += outputValue;
            }

            return Math.Max(-1.0F, Math.Min(1.0F, result));
        }

        private float AdjustOutputValue(float outputValue, ControllerAction controllerAction)
        {
            if (controllerAction.ChannelOutputType == ChannelOutputType.NormalMotor && controllerAction.MaxOutputPercent < 100)
            {
                outputValue = (outputValue * controllerAction.MaxOutputPercent) / 100;
            }

            return controllerAction.IsInvert ? -outputValue : outputValue;
        }
    }
}
