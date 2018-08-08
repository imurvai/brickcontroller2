using System;
using BrickController2.HardwareServices;

namespace BrickController2.iOS.HardwareServices
{
    public class GameControllerService : IGameControllerService
    {
        public event EventHandler<GameControllerEventArgs> GameControllerEvent;
    }
}