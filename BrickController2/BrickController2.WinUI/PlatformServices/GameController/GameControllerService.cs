using BrickController2.PlatformServices.GameController;
using BrickController2.UI.Services.MainThread;
using BrickController2.Windows.Extensions;
using Microsoft.Maui.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Gaming.Input;

namespace BrickController2.Windows.PlatformServices.GameController;

public class GameControllerService : IGameControllerService
{

    private readonly Dictionary<string, GamepadController> _availableControllers = new();
    private readonly object _lockObject = new();
    private readonly IMainThreadService _mainThreadService;
    private readonly IDispatcherProvider _dispatcherProvider;

    private event EventHandler<GameControllerEventArgs> GameControllerEventInternal;

    public GameControllerService(IMainThreadService mainThreadService, IDispatcherProvider dispatcherProvider)
    {
        _mainThreadService = mainThreadService;
        _dispatcherProvider = dispatcherProvider;
    }

    public event EventHandler<GameControllerEventArgs> GameControllerEvent
    {
        add
        {
            lock (_lockObject)
            {
                if (GameControllerEventInternal == null)
                {
                    InitializeControllers();
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
                    TerminateControllers();
                }
            }
        }
    }

    internal void RaiseEvent(IDictionary<(GameControllerEventType, string), float> events)
    {
        if (!events.Any())
        {
            return;
        }

        GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(events));
    }

    internal void RaiseEvent(string deviceId, string key, GameControllerEventType eventType, float value = 0.0f)
    {
        GameControllerEventInternal?.Invoke(this, new GameControllerEventArgs(eventType, key, value));
    }

    private void InitializeControllers()
    {
        // get all available gamepads
        if (Gamepad.Gamepads.Any())
        {
            AddDevices(Gamepad.Gamepads);
        }

        Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded += Gamepad_GamepadAdded;
    }

    private void TerminateControllers()
    {
        Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
        Gamepad.GamepadAdded -= Gamepad_GamepadAdded;

        foreach (var controller in _availableControllers.Values)
        {
            controller.Stop();
        }
        _availableControllers.Clear();
    }

    private void Gamepad_GamepadRemoved(object sender, Gamepad e)
    {
        lock (_lockObject)
        {
            var deviceId = e.GetDeviceId();

            if (_availableControllers.TryGetValue(deviceId, out var controller))
            {
                _availableControllers.Remove(deviceId);

                // ensure stopped in UI thread
                _ = _mainThreadService.RunOnMainThread(() => controller.Stop());
            }
        }
    }

    private void Gamepad_GamepadAdded(object sender, Gamepad e)
    {
        // ensure created in UI thread
        _ = _mainThreadService.RunOnMainThread(() => AddDevices(new[] { e }));
    }

    private void AddDevices(IEnumerable<Gamepad> gamepads)
    {
        lock (_lockObject)
        {
            var dispatcher = _dispatcherProvider.GetForCurrentThread();
            foreach (var gamepad in gamepads)
            {
                var deviceId = gamepad.GetDeviceId();

                var newController = new GamepadController(this, gamepad, dispatcher.CreateTimer());
                _availableControllers[deviceId] = newController;

                newController.Start();
            }
        }
    }
}