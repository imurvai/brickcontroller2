using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Views;
using BrickController2.HardwareServices.GameController;

namespace BrickController2.Droid.HardwareServices.GameController
{
    public class GameControllerService : IGameControllerService
    {
        private readonly IDictionary<Axis, float> _lastAxisValues = new Dictionary<Axis, float>();
        private readonly object _lockObject = new object();

        private event EventHandler<GameControllerEventArgs> GameControllerEventInternal;

        public event EventHandler<GameControllerEventArgs> GameControllerEvent
        {
            add
            {
                lock (_lockObject)
                {
                    if (GameControllerEventInternal == null)
                    {
                        _lastAxisValues.Clear();
                    }

                    GameControllerEventInternal += value;
                }
            }

            remove
            {
                lock (_lockObject)
                {
                    GameControllerEventInternal -= value;
                }
            }
        }

        public bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if ((((int)e.Source & (int)InputSourceType.Gamepad) == (int)InputSourceType.Gamepad) && e.RepeatCount == 0)
            {
                GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Button, e.KeyCode.ToString(), 1.0F));
                return true;
            }

            return false;
        }

        public bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if ((((int)e.Source & (int)InputSourceType.Gamepad) == (int)InputSourceType.Gamepad) && e.RepeatCount == 0)
            {
                GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Button, e.KeyCode.ToString(), 0.0F));
                return true;
            }

            return false;
        }

        public bool OnGenericMotionEvent(MotionEvent e)
        {
            if (e.Source == InputSourceType.Joystick && e.Action == MotionEventActions.Move)
            {
                var events = new Dictionary<(GameControllerEventType, string), float>();
                foreach (Axis axisCode in Enum.GetValues(typeof(Axis)))
                {
                    var axisValue = e.GetAxisValue(axisCode);
                    if (Math.Abs(axisValue) < 0.05F)
                    {
                        axisValue = 0.0F;
                    }

                    if (_lastAxisValues.TryGetValue(axisCode, out float lastValue))
                    {
                        if (Math.Abs(axisValue - lastValue) < 0.05)
                        {
                            // axisValue == lastValue
                            continue;
                        }
                    }

                    _lastAxisValues[axisCode] = axisValue;
                    events[(GameControllerEventType.Axis, axisCode.ToString())] = axisValue;
                }

                GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(events));
                return true;
            }

            return false;
        }
    }
}