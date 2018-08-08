using System;

namespace BrickController2.HardwareServices
{
    public interface IGameControllerService
    {
        event EventHandler<GameControllerEventArgs> GameControllerEvent;
    }
}
