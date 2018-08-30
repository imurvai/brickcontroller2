using BrickController2.HardwareServices;
using BrickController2.UI.Services;

namespace BrickController2.Droid.UI.Services
{
    public class GameControllerEventDialogResult : IGameControllerEventDialogResult
    {
        public bool IsOk { get; set; }
        public GameControllerEventType EventType { get; set; }
        public string EventCode { get; set; }
    }
}