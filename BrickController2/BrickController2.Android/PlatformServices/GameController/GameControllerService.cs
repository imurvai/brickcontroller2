using Android.Runtime;
using Android.Views;
using BrickController2.PlatformServices.GameController;

namespace BrickController2.Droid.PlatformServices.GameController
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

                    if ((axisCode == Axis.Rx || axisCode == Axis.Ry) &&
                        e.Device.VendorId == 1356 &&
                        (e.Device.ProductId == 2508 || e.Device.ProductId == 1476))
                    {
                        // DualShock 4 hack for the triggers ([-1:1] -> [0:1])
                        if (!_lastAxisValues.ContainsKey(axisCode) && axisValue == 0.0F)
                        {
                            continue;
                        }

                        axisValue = (axisValue + 1) / 2;
                    }

                    if (e.Device.VendorId == 0x057e &&
                        (/*e.Device.ProductId == 0x2006 || e.Device.ProductId == 0x2007 ||*/ e.Device.ProductId == 0x2009))
                    {
                        // Nintendo Switch Pro controller hack ([-0.69:0.7] -> [-1:1])
                        // 2006 and 2007 are for the Nintendo Joy-Con controller (haven't reported issues with it)
                        axisValue = Math.Min(1, Math.Max(-1, axisValue / 0.69F));
                    }

                    if (e.Device.VendorId == 1118 && e.Device.ProductId == 765 &&
                        axisCode == Axis.Generic1)
                    {
                        // XBox One controller reports a constant value on Generic 1 - filter it out
                        continue;
                    }

                    axisValue = AdjustControllerValue(axisValue);

                    if (_lastAxisValues.TryGetValue(axisCode, out float lastValue))
                    {
                        if (AreAlmostEqual(axisValue, lastValue))
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