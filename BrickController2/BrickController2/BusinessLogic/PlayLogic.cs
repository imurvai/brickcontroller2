using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using DeviceType = BrickController2.DeviceManagement.DeviceType;

namespace BrickController2.BusinessLogic
{
    public class PlayLogic : IPlayLogic
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly ISequencePlayer _sequencePlayer;

        private readonly IDictionary<(string DeviceId, int Channel), float[]> _previousOutputs = new Dictionary<(string, int), float[]>();
        private readonly IDictionary<(string EventCode, string DeviceId, int Channel), float> _previousAxisOutputs = new Dictionary<(string, string, int), float>();
        private readonly IDictionary<(string DeviceId, int Channel), bool> _disabledOutputForAxises = new Dictionary<(string, int), bool>();
        private readonly IDictionary<(string DeviceId, int Channel), IDictionary<(GameControllerEventType EventType, string EventCode), float>> _axisOutputValues = new Dictionary<(string, int), IDictionary<(GameControllerEventType, string), float>>();

        public PlayLogic(
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            ISequencePlayer sequencePlayer)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _sequencePlayer = sequencePlayer;
        }

        public ControllerProfile ActiveProfile { get; set; }

        public CreationValidationResult ValidateCreation(Creation creation)
        {
            var deviceIds = creation.GetDeviceIds();
            var sequenceNames = creation.GetSequenceNames();

            if (deviceIds == null || deviceIds.Count() == 0)
            {
                return CreationValidationResult.MissingControllerAction;
            }
            else if (deviceIds.Any(di => _deviceManager.GetDeviceById(di) == null))
            {
                return CreationValidationResult.MissingDevice;
            }
            else if (sequenceNames != null && sequenceNames.Any(sn => _creationManager.Sequences.FirstOrDefault(s => s.Name == sn) == null))
            {
                return CreationValidationResult.MissingSequence;
            }

            return CreationValidationResult.Ok;
        }

        public bool ValidateControllerAction(ControllerAction controllerAction)
        {
            var device = _deviceManager.GetDeviceById(controllerAction.DeviceId);
            var sequence = _creationManager.Sequences.FirstOrDefault(s => s.Name == controllerAction.SequenceName);

            return device != null && (controllerAction.ButtonType != ControllerButtonType.Sequence || sequence != null);
        }

        public void StartPlay()
        {
            _sequencePlayer.StartPlayer();
        }

        public void StopPlay()
        {
            _sequencePlayer.StopPlayer();
        }

        public void ProcessGameControllerEvent(GameControllerEventArgs e)
        {
            if (ActiveProfile == null)
            {
                return;
            }

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

                            if (gameControllerEvent.Key.EventType == GameControllerEventType.Button)
                            {
                                var isPressed = gameControllerEvent.Value > 0.5;
                                if (!ShouldProcessButtonEvent(isPressed, controllerAction))
                                {
                                    continue;
                                }

                                var outputValue = ProcessButtonEvent(isPressed, controllerAction, device.DeviceType);
                                device.SetOutput(channel, outputValue);
                            }
                            else if (gameControllerEvent.Key.EventType == GameControllerEventType.Axis)
                            {
                                var (useAxisValue, axisValue) = ProcessAxisEvent(gameControllerEvent.Key.EventCode, gameControllerEvent.Value, controllerAction, device.DeviceType);
                                if (useAxisValue)
                                {
                                    StoreAxisOutputValue(axisValue, controllerAction.DeviceId, controllerAction.Channel, controllerEvent.EventType, controllerEvent.EventCode);
                                    var outputValue = CombineAxisOutputValues(controllerAction.DeviceId, controllerAction.Channel);
                                    device.SetOutput(channel, outputValue);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool ShouldProcessButtonEvent(bool isPressed, ControllerAction controllerAction)
        {
            return controllerAction.ButtonType == ControllerButtonType.Normal || isPressed;
        }

        private float ProcessButtonEvent(bool isPressed, ControllerAction controllerAction, DeviceType deviceType)
        {
            var previousOutputs = GetPreviousOutputs(controllerAction);
            float currentOutput = 0;
            float buttonValue = isPressed ? (controllerAction.IsInvert ? -1 : 1) : 0;

            switch (controllerAction.ButtonType)
            {
                case ControllerButtonType.Normal:
                    currentOutput = buttonValue;
                    break;

                case ControllerButtonType.SimpleToggle:
                    currentOutput = previousOutputs[0] != 0 ? 0 : buttonValue;
                    break;

                case ControllerButtonType.Alternating:
                    currentOutput = (previousOutputs[0] * buttonValue) <= 0 ? buttonValue : -buttonValue;
                    break;

                case ControllerButtonType.Circular:
                    currentOutput = previousOutputs[0] += buttonValue;
                    if (currentOutput > 1)
                    {
                        currentOutput = -1;
                    }
                    else if (currentOutput < -1)
                    {
                        currentOutput = 1;
                    }
                    break;

                case ControllerButtonType.PingPong:
                    if (previousOutputs[0] != 0)
                    {
                        currentOutput = 0;
                    }
                    else
                    {
                        currentOutput = (previousOutputs[1] * buttonValue) <= 0 ? buttonValue : -buttonValue;
                    }
                    break;

                case ControllerButtonType.Stop:
                    currentOutput = 0;

                    SetIsOutputDisabledForAxises(controllerAction, false);
                    ResetPreviousAxisOutputsForOutput(controllerAction);

                    break;

                case ControllerButtonType.Accelerator:
                    var accelarationStep = GetAccelarationStep(deviceType);
                    accelarationStep = controllerAction.IsInvert ? -accelarationStep : accelarationStep;
                    currentOutput = Math.Min(Math.Max(previousOutputs[0] + accelarationStep, -1), 1);
                    break;

                case ControllerButtonType.Sequence:
                    var sequence = _creationManager.Sequences.FirstOrDefault(s => s.Name == controllerAction.SequenceName);
                    if (sequence != null)
                    {
                        _sequencePlayer.ToggleSequence(controllerAction.DeviceId, controllerAction.Channel, controllerAction.IsInvert, sequence);
                    }
                    break;
            }

            SetPreviousOutput(controllerAction, currentOutput);
            return AdjustOutputValue(currentOutput, controllerAction);
        }

        private float[] GetPreviousOutputs(ControllerAction controllerAction)
        {
            if (_previousOutputs.ContainsKey((controllerAction.DeviceId, controllerAction.Channel)))
            {
                return _previousOutputs[(controllerAction.DeviceId, controllerAction.Channel)];
            }
            else
            {
                var prevOutputs = new float[2] { 0, 0 };
                _previousOutputs[(controllerAction.DeviceId, controllerAction.Channel)] = prevOutputs;
                return prevOutputs;
            }
        }

        private void SetPreviousOutput(ControllerAction controllerAction, float value)
        {
            var buttonOutputs = _previousOutputs[(controllerAction.DeviceId, controllerAction.Channel)];
            buttonOutputs[1] = buttonOutputs[0];
            buttonOutputs[0] = value;
        }

        private (bool UseAxisValue, float AxisValue) ProcessAxisEvent(string gameControllerEventCode, float axisValue, ControllerAction controllerAction, DeviceType deviceType)
        {
            var previousAxisValue = GetPreviousAxisOutput(gameControllerEventCode, controllerAction);

            axisValue = controllerAction.IsInvert ? -axisValue : axisValue;

            var axisDeadZone = controllerAction.AxisDeadZonePercent / 100F;
            if (axisDeadZone > 0)
            {
                if (Math.Abs(axisValue) <= axisDeadZone)
                {
                    return (true, 0);
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

            axisValue = controllerAction.AxisActiveZonePercent > 0 ? axisValue * 100F / controllerAction.AxisActiveZonePercent : axisValue;

            if (controllerAction.AxisCharacteristic == ControllerAxisCharacteristic.Exponential)
            {
                // Cheat :)
                axisValue = axisValue * axisValue * axisValue;
            }
            else if (controllerAction.AxisCharacteristic == ControllerAxisCharacteristic.Logarithmic)
            {
                // Another cheat :)
                if (axisValue < 0)
                {
                    axisValue = -(float)Math.Pow(Math.Abs(axisValue), 1F / 3);
                }
                else
                {
                    axisValue = (float)Math.Pow(axisValue, 1F / 3);
                }
            }

            var useAxisValue = true;

            switch (controllerAction.AxisType)
            {
                case ControllerAxisType.Train:
                    if (GetIsOutputDisableForAxises(controllerAction))
                    {
                        if (axisValue == 0)
                        {
                            SetIsOutputDisabledForAxises(controllerAction, false);
                        }

                        useAxisValue = false;
                    }
                    else if (previousAxisValue != 0)
                    {
                        if (Math.Sign(axisValue) == Math.Sign(previousAxisValue))
                        {
                            // The sign of axisValue and previouAxisValue are same
                            if (Math.Abs(axisValue) < Math.Abs(previousAxisValue))
                            {
                                // Don't accelarate
                                useAxisValue = false;
                            }
                        }
                        else
                        {
                            // The sign of axisValue and previousAxisValue are different
                            if (Math.Abs(previousAxisValue - axisValue) < 1)
                            {
                                // Don't slow down
                                useAxisValue = false;
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

                    break;

                case ControllerAxisType.Accelerator:
                    if (Math.Abs(axisValue) == 1)
                    {
                        var accelarationStep = GetAccelarationStep(deviceType);
                        axisValue = Math.Min(Math.Max(previousAxisValue + (axisValue * accelarationStep), -1), 1);
                    }
                    else
                    {
                        useAxisValue = false;
                    }

                    break;
            }

            if (useAxisValue)
            {
                SetPreviousAxisOutput(gameControllerEventCode, controllerAction, axisValue);
                axisValue = AdjustOutputValue(axisValue, controllerAction);
            }

            return (useAxisValue, axisValue);
        }

        private float GetPreviousAxisOutput(string gameControllerEventCode, ControllerAction controllerAction)
        {
            if (_previousAxisOutputs.ContainsKey((gameControllerEventCode, controllerAction.DeviceId, controllerAction.Channel)))
            {
                return _previousAxisOutputs[(gameControllerEventCode, controllerAction.DeviceId, controllerAction.Channel)];
            }
            else
            {
                var prevOutput = 0.0f;
                _previousAxisOutputs[(gameControllerEventCode, controllerAction.DeviceId, controllerAction.Channel)] = prevOutput;
                return prevOutput;
            }
        }

        private void SetPreviousAxisOutput(string gameControllerEventCode, ControllerAction controllerAction, float value)
        {
            _previousAxisOutputs[(gameControllerEventCode, controllerAction.DeviceId, controllerAction.Channel)] = value;
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
            if (!_disabledOutputForAxises.ContainsKey((controllerAction.DeviceId, controllerAction.Channel)))
            {
                _disabledOutputForAxises[(controllerAction.DeviceId, controllerAction.Channel)] = false;
            }

            return _disabledOutputForAxises[(controllerAction.DeviceId, controllerAction.Channel)];
        }

        private void SetIsOutputDisabledForAxises(ControllerAction controllerAction, bool value)
        {
            _disabledOutputForAxises[(controllerAction.DeviceId, controllerAction.Channel)] = value;
        }

        private void ResetPreviousAxisOutputsForOutput(ControllerAction controllerAction)
        {
            foreach (var key in _previousAxisOutputs.Keys.Where(k => k.DeviceId == controllerAction.DeviceId && k.Channel == controllerAction.Channel).ToArray())
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

            return outputValue;
        }

        private float GetAccelarationStep(DeviceType deviceType)
        {
            switch (deviceType)
            {
                case DeviceType.BuWizz:
                case DeviceType.BuWizz2:
                case DeviceType.Infrared:
                case DeviceType.SBrick:
                    return 1F / 7;

                default:
                    return 0.1F;
            }
        }
    }
}
