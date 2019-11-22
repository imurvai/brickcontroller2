using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.GameController;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrickController2.BusinessLogic
{
    public class PlayLogic
    {
        private readonly IDeviceManager _deviceManager;

        private readonly IDictionary<(string EventCode, ControllerAction ControllerAction), float[]> _previousButtonOutputs = new Dictionary<(string, ControllerAction), float[]>();
        private readonly IDictionary<(string EventCode, ControllerAction ControllerAction), float> _previousAxisOutputs = new Dictionary<(string, ControllerAction), float>();
        private readonly IDictionary<ControllerAction, bool> _disabledOutputForAxises = new Dictionary<ControllerAction, bool>();
        private readonly IDictionary<(string DeviceId, int Channel), IDictionary<(GameControllerEventType EventType, string EventCode), float>> _axisOutputValues = new Dictionary<(string, int), IDictionary<(GameControllerEventType, string), float>>();

        public PlayLogic(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public ControllerProfile ActiveProfile { get; set; }

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

                                var outputValue = ProcessButtonEvent(gameControllerEvent.Key.EventCode, isPressed, controllerAction);
                                device.SetOutput(channel, outputValue);
                            }
                            else if (gameControllerEvent.Key.EventType == GameControllerEventType.Axis)
                            {
                                var (useAxisValue, axisValue) = ProcessAxisEvent(gameControllerEvent.Key.EventCode, gameControllerEvent.Value, controllerAction);
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

                case ControllerButtonType.Accelerator:

                    // TODO:
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

        private (bool UseAxisValue, float AxisValue) ProcessAxisEvent(string gameControllerEventCode, float axisValue, ControllerAction controllerAction)
        {
            var previousAxisValue = GetPreviousAxisOutput(gameControllerEventCode, controllerAction);

            var axisDeadZone = controllerAction.AxisDeadZonePercent / 100F;
            if (axisDeadZone > 0)
            {
                if (Math.Abs(axisValue) <= axisDeadZone)
                {
                    return (false, 0);
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

            if (controllerAction.AxisType == ControllerAxisType.Train)
            {
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
