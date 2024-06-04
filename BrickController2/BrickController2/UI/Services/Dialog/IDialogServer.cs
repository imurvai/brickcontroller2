using BrickController2.PlatformServices.GameController;

namespace BrickController2.UI.Services.Dialog
{
    public interface IDialogServer : IDialogService
    {
        IGameControllerService? GameControllerService { get; set; }
    }
}
