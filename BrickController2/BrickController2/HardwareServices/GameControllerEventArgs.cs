using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.HardwareServices
{
    public enum GameControllerEventType
    {
        Button,
        Axis
    }

    public class GameControllerEventArgs : EventArgs
    {
        public GameControllerEventArgs(GameControllerEventType eventType, string eventCode, float value)
        {
            var events = new Dictionary<(GameControllerEventType, string), float>();
            events[(eventType, eventCode)] = value;
            ControllerEvents = events;
        }

        public GameControllerEventArgs(IDictionary<(GameControllerEventType, string), float> events)
        {
            ControllerEvents = new ReadOnlyDictionary<(GameControllerEventType, string), float>(events);
        }

        public IReadOnlyDictionary<(GameControllerEventType eventType, string eventCode), float> ControllerEvents { get; }
    }
}
