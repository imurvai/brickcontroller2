using System;
using System.Collections.Generic;
using Android.Runtime;
using Android.Views;
using BrickController2.HardwareServices;

namespace BrickController2.Droid.HardwareServices
{
    public class GameControllerService : IGameControllerService
    {
        public event EventHandler<GameControllerEventArgs> GameControllerEvent;

        public bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if ((((int)e.Source & (int)InputSourceType.Gamepad) == (int)InputSourceType.Gamepad) && e.RepeatCount == 0)
            {
                GameControllerEvent?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Button, e.KeyCode.ToString(), 1.0F));
                return true;
            }

            return false;
        }

        public bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if ((((int)e.Source & (int)InputSourceType.Gamepad) == (int)InputSourceType.Gamepad) && e.RepeatCount == 0)
            {
                GameControllerEvent?.Invoke(this, new GameControllerEventArgs(GameControllerEventType.Button, e.KeyCode.ToString(), 0.0F));
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

                    if (Math.Abs(axisValue) < 0.1F)
                    {
                        axisValue = 0.0F;
                    }

                    events[(GameControllerEventType.Axis, axisCode.ToString())] = axisValue;
                }

                GameControllerEvent?.Invoke(this, new GameControllerEventArgs(events));
                return true;
            }

            return false;
        }
    }
}