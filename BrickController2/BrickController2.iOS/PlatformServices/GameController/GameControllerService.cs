using System;
using System.Collections.Generic;
using System.Linq;
using BrickController2.PlatformServices.GameController;
using Foundation;
using GameController;

namespace BrickController2.iOS.PlatformServices.GameController
{
    public class GameControllerService : IGameControllerService
    {
        private enum GameControllerType
        {
            Unknown,
            Micro,
            Standard,
            Extended
        };

        private readonly object _lockObject = new object();

        private readonly IDictionary<string, float> _lastControllerEventValueMap = new Dictionary<string, float>();

        private GCController _gameController;
        private event EventHandler<GameControllerEventArgs> GameControllerEventInternal;
        private NSObject _didConnectNotification;
        private NSObject _didDisconnectNotification;

        public event EventHandler<GameControllerEventArgs> GameControllerEvent
        {
            add
            {
                lock (_lockObject)
                {
                    if (GameControllerEventInternal == null)
                    {
                        if (GCController.Controllers.Length == 0)
                        {
                            FindController();
                        }
                        else
                        {
                            FoundController();
                        }
                    }

                    GameControllerEventInternal += value;
                }
            }

            remove
            {
                lock (_lockObject)
                {
                    GameControllerEventInternal -= value;

                    if (GameControllerEventInternal == null)
                    {
                        GCController.StopWirelessControllerDiscovery();
                        _didConnectNotification?.Dispose();
                        _didDisconnectNotification?.Dispose();
                        _didConnectNotification = null;
                        _didDisconnectNotification = null;
                        _gameController?.Dispose();
                        _gameController = null;
                    }
                }
            }
        }

        private void FindController()
        {
            lock (_lockObject)
            {
                _didConnectNotification = GCController.Notifications.ObserveDidConnect((sender, args) =>
                {
                    FoundController();
                });

                GCController.StartWirelessControllerDiscovery(() => { });
            }
        }

        private void FoundController()
        {
            lock (_lockObject)
            {
                _gameController = GCController.Controllers.FirstOrDefault();

                if (_gameController != null)
                {
                    GCController.StopWirelessControllerDiscovery();
                    _didConnectNotification?.Dispose();
                    _didConnectNotification = null;

                    _didDisconnectNotification = GCController.Notifications.ObserveDidDisconnect((sender, args) =>
                    {
                        FindController();
                    });

                    switch (GetGameControllerType(_gameController))
                    {
                        case GameControllerType.Micro:
                            SetupMicroGamePad(_gameController.MicroGamepad);
                            break;

                        case GameControllerType.Standard:
                            SetupGamePad(_gameController.Gamepad);
                            break;

                        case GameControllerType.Extended:
                            SetupExtendedGamePad(_gameController.ExtendedGamepad);
                            break;
                    }
                }
            }
        }

        private GameControllerType GetGameControllerType(GCController controller)
        {
            try
            {
                if (controller.MicroGamepad != null)
                {
                    return GameControllerType.Micro;
                }
            }
            catch (InvalidCastException) { }

            try
            {
                if (controller.Gamepad != null)
                {
                    return GameControllerType.Standard;
                }
            }
            catch (InvalidCastException) { }

            try
            {
                if (controller.ExtendedGamepad != null)
                {
                    return GameControllerType.Extended;
                }
            }
            catch (InvalidCastException) { }

            return GameControllerType.Unknown;
        }

        private void SetupMicroGamePad(GCMicroGamepad gamePad)
        {
            SetupDigitalButtonInput(gamePad.ButtonA, "Button_A");
            SetupDigitalButtonInput(gamePad.ButtonX, "Button_X");

            SetupDPadInput(gamePad.Dpad, "DPad");
        }

        private void SetupGamePad(GCGamepad gamePad)
        {
            SetupDigitalButtonInput(gamePad.ButtonA, "Button_A");
            SetupDigitalButtonInput(gamePad.ButtonB, "Button_B");
            SetupDigitalButtonInput(gamePad.ButtonX, "Button_X");
            SetupDigitalButtonInput(gamePad.ButtonY, "Button_Y");

            SetupDigitalButtonInput(gamePad.LeftShoulder, "LeftShoulder");
            SetupDigitalButtonInput(gamePad.RightShoulder, "RightShoulder");

            SetupDPadInput(gamePad.DPad, "DPad");
        }

        private void SetupExtendedGamePad(GCExtendedGamepad gamePad)
        {
            SetupDigitalButtonInput(gamePad.ButtonA, "Button_A");
            SetupDigitalButtonInput(gamePad.ButtonB, "Button_B");
            SetupDigitalButtonInput(gamePad.ButtonX, "Button_X");
            SetupDigitalButtonInput(gamePad.ButtonY, "Button_Y");

            SetupDigitalButtonInput(gamePad.LeftShoulder, "LeftShoulder");
            SetupDigitalButtonInput(gamePad.RightShoulder, "RightShoulder");

            SetupAnalogButtonInput(gamePad.LeftTrigger, "LeftTrigger");
            SetupAnalogButtonInput(gamePad.RightTrigger, "RightTrigger");

            SetupDPadInput(gamePad.DPad, "DPad");

            SetupJoyInput(gamePad.LeftThumbstick, "LeftThumbStick");
            SetupJoyInput(gamePad.RightThumbstick, "RightThumbStick");
        }

        private void SetupDigitalButtonInput(GCControllerButtonInput button, string name)
        {
            button.ValueChangedHandler = (btn, value, isPressed) =>
            {
                value = isPressed ? 1.0F : 0.0F;

                if (!_lastControllerEventValueMap.ContainsKey(name) || !AreAlmostEqual(_lastControllerEventValueMap[name], value))
                {
                    _lastControllerEventValueMap[name] = value;
                    GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Button, name, value));
                }
            };
        }

        private void SetupAnalogButtonInput(GCControllerButtonInput button, string name)
        {
            button.ValueChangedHandler = (btn, value, isPressed) =>
            {
                value = value < 0.1 ? 0.0F : value;

                if (!_lastControllerEventValueMap.ContainsKey(name) || !AreAlmostEqual(_lastControllerEventValueMap[name], value))
                {
                    _lastControllerEventValueMap[name] = value;
                    GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Axis, name, value));
                }
            };
        }

        private void SetupDPadInput(GCControllerDirectionPad dPad, string name)
        {
            SetupDigitalAxisInput(dPad.XAxis, $"{name}_X");
            SetupDigitalAxisInput(dPad.YAxis, $"{name}_Y");
        }

        private void SetupDigitalAxisInput(GCControllerAxisInput axis, string name)
        {
            axis.ValueChangedHandler = (ax, value) =>
            {
                if (value < -0.1F) value = -1.0F;
                else if (value > 0.1F) value = 1.0F;
                else value = 0.0F;

                if (!_lastControllerEventValueMap.ContainsKey(name) || !AreAlmostEqual(_lastControllerEventValueMap[name], value))
                {
                    GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Axis, name, value));
                    _lastControllerEventValueMap[name] = value;
                }
            };
        }

        private void SetupJoyInput(GCControllerDirectionPad joy, string name)
        {
            SetupAnalogAxisInput(joy.XAxis, $"{name}_X");
            SetupAnalogAxisInput(joy.YAxis, $"{name}_Y");
        }

        private void SetupAnalogAxisInput(GCControllerAxisInput axis, string name)
        {
            axis.ValueChangedHandler = (ax, value) =>
            {
                value = AdjustControllerValue(value);

                if (!_lastControllerEventValueMap.ContainsKey(name) || !AreAlmostEqual(_lastControllerEventValueMap[name], value))
                {
                    GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Axis, name, value));
                    _lastControllerEventValueMap[name] = value;
                }
            };
        }

        private float AdjustControllerValue(float value)
        {
            value = Math.Abs(value) < 0.05 ? 0.0F : value;
            value = value > 0.95 ? 1.0F : value;
            value = value < -0.95 ? -1.0F : value;
            return value;
        }

        private bool AreAlmostEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.001;
        }
    }
}