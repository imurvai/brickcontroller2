using BrickController2.HardwareServices;

namespace BrickController2.UI.Services
{
    public interface IGameControllerEventDialogResult
    {
        bool IsOk { get; }
        GameControllerEventType EventType { get; }
        string EventCode { get; }
    }
}
