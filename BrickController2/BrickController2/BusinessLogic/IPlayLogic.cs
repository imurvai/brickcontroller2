using BrickController2.CreationManagement;
using BrickController2.PlatformServices.GameController;
using System.Threading.Tasks;

namespace BrickController2.BusinessLogic
{
    public interface IPlayLogic
    {
        ControllerProfile ActiveProfile { get; set; }

        CreationValidationResult ValidateCreation(Creation creation);

        Task StartPlayAsync();
        Task StopPlayAsync();

        void ProcessGameControllerEvent(GameControllerEventArgs e);
    }
}
