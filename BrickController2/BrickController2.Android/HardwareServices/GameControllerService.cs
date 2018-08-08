using System;
using System.Collections.Generic;
using BrickController2.HardwareServices;

namespace BrickController2.Droid.HardwareServices
{
    public class GameControllerService : IGameControllerService
    {
        public event EventHandler<GameControllerEventArgs> GameControllerEvent;

        public void SendEvent(GameControllerEventType eventType, string eventCode, float value)
        {
            GameControllerEvent?.Invoke(this, new GameControllerEventArgs(eventType, eventCode, value));
        }

        public void SendEvents(IDictionary<(GameControllerEventType, string), float> events)
        {
            GameControllerEvent?.Invoke(this, new GameControllerEventArgs(events));
        }
    }
}