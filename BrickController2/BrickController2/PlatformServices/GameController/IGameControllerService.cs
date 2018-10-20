using System;

namespace BrickController2.PlatformServices.GameController
{
    public interface IGameControllerService
    {
        event EventHandler<GameControllerEventArgs> GameControllerEvent;
    }
}
