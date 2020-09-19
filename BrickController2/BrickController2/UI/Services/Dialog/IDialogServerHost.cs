namespace BrickController2.UI.Services.Dialog
{
    public interface IDialogServerHost
    {
        void RegisterDialogServer(IDialogServer dialogServer);
        void UnregisterDialogServer(IDialogServer dialogServer);
    }
}
