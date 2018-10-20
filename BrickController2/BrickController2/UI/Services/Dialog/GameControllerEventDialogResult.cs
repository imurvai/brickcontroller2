using BrickController2.PlatformServices.GameController;

namespace BrickController2.UI.Services.Dialog
{
    public class GameControllerEventDialogResult
    {
        public GameControllerEventDialogResult(bool isOk, GameControllerEventType eventType, string eventCode)
        {
            IsOk = isOk;
            EventType = eventType;
            EventCode = eventCode;
        }

        public bool IsOk { get; }
        public GameControllerEventType EventType { get; }
        public string EventCode { get; }
    }
}
