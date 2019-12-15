using BrickController2.CreationManagement;
using BrickController2.PlatformServices.GameController;

namespace BrickController2.BusinessLogic
{
    public interface IPlayLogic
    {
        ControllerProfile ActiveProfile { get; set; }

        void ProcessGameControllerEvent(GameControllerEventArgs e);
    }
}
