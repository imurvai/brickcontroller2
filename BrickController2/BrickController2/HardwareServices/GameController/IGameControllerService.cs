using System;

namespace BrickController2.HardwareServices.GameController
{
    public interface IGameControllerService
    {
        event EventHandler<GameControllerEventArgs> GameControllerEvent;
    }
}
