using System;
using System.Collections.Generic;
using System.Linq;
using BrickController2.HardwareServices;
using Foundation;
using GameController;

namespace BrickController2.iOS.HardwareServices
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
        private EventHandler<GameControllerEventArgs> _gameControllerEventHandler;
        private NSObject _didConnectNotification;
        private NSObject _didDisconnectNotification;

        public event EventHandler<GameControllerEventArgs> GameControllerEvent
        {
            add
            {
                lock (_lockObject)
                {
                    if (_gameControllerEventHandler == null)
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

                    _gameControllerEventHandler += value;
                }
            }

            remove
            {
                lock (_lockObject)
                {
                    _gameControllerEventHandler -= value;

                    if (_gameControllerEventHandler == null)
                    {
                        GCController.StopWirelessControllerDiscovery();
                        _didConnectNotification?.Dispose();
                        _didDisconnectNotification?.Dispose();
                        _didConnectNotification = null;
                        _didDisconnectNotification = null;
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

                GCController.StartWirelessControllerDiscoveryAsync();
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
            SetupButtonInput(gamePad.ButtonA, "Button_A");
            SetupButtonInput(gamePad.ButtonX, "Button_X");

            SetupDPadInput(gamePad.Dpad, "DPad");
        }

        private void SetupGamePad(GCGamepad gamePad)
        {
            SetupButtonInput(gamePad.ButtonA, "Button_A");
            SetupButtonInput(gamePad.ButtonB, "Button_B");
            SetupButtonInput(gamePad.ButtonX, "Button_X");
            SetupButtonInput(gamePad.ButtonY, "Button_Y");

            SetupButtonInput(gamePad.LeftShoulder, "LeftShoulder");
            SetupButtonInput(gamePad.RightShoulder, "LeftShoulder");

            SetupDPadInput(gamePad.DPad, "DPad");
        }

        private void SetupExtendedGamePad(GCExtendedGamepad gamePad)
        {
            SetupButtonInput(gamePad.ButtonA, "Button_A");
            SetupButtonInput(gamePad.ButtonB, "Button_B");
            SetupButtonInput(gamePad.ButtonX, "Button_X");
            SetupButtonInput(gamePad.ButtonY, "Button_Y");

            SetupButtonInput(gamePad.LeftShoulder, "LeftShoulder");
            SetupButtonInput(gamePad.RightShoulder, "LeftShoulder");

            SetupButtonInput(gamePad.LeftTrigger, "LeftTrigger");
            SetupButtonInput(gamePad.RightTrigger, "RightTrigger");

            SetupDPadInput(gamePad.DPad, "DPad");

            SetupDPadInput(gamePad.LeftThumbstick, "Axis_LeftThumbStick");
            SetupDPadInput(gamePad.RightThumbstick, "Axis_RightThumbStick");
        }

        private void SetupButtonInput(GCControllerButtonInput button, string name)
        {
            button.ValueChangedHandler = (btn, value, isPressed) =>
            {
                if (!btn.IsAnalog)
                {
                    value = btn.IsPressed ? 1.0F : 0.0F;
                }

                if (!_lastControllerEventValueMap.ContainsKey(name) || !AreAlmostEqual(_lastControllerEventValueMap[name], value))
                {
                    _lastControllerEventValueMap[name] = value;
                    _gameControllerEventHandler?.Invoke(this, new GameControllerEventArgs(btn.IsAnalog ? GameControllerEventType.Axis : GameControllerEventType.Button, name, value));
                }
            };
        }

        private void SetupDPadInput(GCControllerDirectionPad dPad, string name)
        {
            SetupAxisInput(dPad.XAxis, $"{name}_X");
            SetupAxisInput(dPad.YAxis, $"{name}_Y");
        }

        private void SetupAxisInput(GCControllerAxisInput axis, string name)
        {
            axis.ValueChangedHandler = (ax, value) =>
            {
                if (!ax.IsAnalog)
                {
                    if (value < -0.5F) value = -1.0F;
                    else if (value > 0.5F) value = 1.0F;
                    else value = 0.0F;
                }

                if (!_lastControllerEventValueMap.ContainsKey(name) || !AreAlmostEqual(_lastControllerEventValueMap[name], value))
                {
                    _gameControllerEventHandler?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Axis, name, value));
                    _lastControllerEventValueMap[name] = value;
                }
            };
        }

        private bool AreAlmostEqual(float a, float b)
        {
            return Math.Abs(a - b) < 0.01;
        }
    }
}