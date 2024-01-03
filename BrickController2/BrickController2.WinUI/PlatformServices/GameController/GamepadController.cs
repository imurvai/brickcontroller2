using BrickController2.PlatformServices.GameController;
using BrickController2.Windows.Extensions;
using Windows.Gaming.Input;

namespace BrickController2.Windows.PlatformServices.GameController;

internal class GamepadController
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(10);

    private readonly GameControllerService _controllerService;
    private readonly Gamepad _gamepad;
    private readonly IDispatcherTimer _timer;

    private readonly Dictionary<string, float> _lastReadingValues = new ();

    public GamepadController(GameControllerService service, Gamepad gamepad, IDispatcherTimer timer)
        : this(service, gamepad, timer, DefaultInterval)
    {
    }

    private GamepadController(GameControllerService service, Gamepad gamepad, IDispatcherTimer timer, TimeSpan timerInterval)
    {
        _controllerService = service;
        _gamepad = gamepad;
        _timer = timer;

        _timer.Interval = timerInterval;
        _timer.Tick += Timer_Tick;
    }

    public string DeviceId => _gamepad.GetDeviceId();

    public void Start()
    {
        _lastReadingValues.Clear();

        // finally start timer
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();

        _lastReadingValues.Clear();
    }

    private void Timer_Tick(object sender, object e)
    {
        var currentReading = _gamepad.GetCurrentReading();

        var currentEvents = currentReading
            .Enumerate()
            .Where(HasChanged)
            .ToDictionary(x => (x.EventType, x.Name), x => x.Value);

        _controllerService.RaiseEvent(currentEvents);
    }

    private static bool AreAlmostEqual(float a, float b)
    {
        return Math.Abs(a - b) < 0.001;
    }

    private bool HasChanged((string AxisName, GameControllerEventType EventType, float Value) readingValue)
    {
        // get last reported value of the default one
        _lastReadingValues.TryGetValue(readingValue.AxisName, out float lastValue);
        // skip value if there is no change
        if (AreAlmostEqual(readingValue.Value, lastValue))
        {
            return false;
        }

        _lastReadingValues[readingValue.AxisName] = readingValue.Value;
        return true;
    }
}
