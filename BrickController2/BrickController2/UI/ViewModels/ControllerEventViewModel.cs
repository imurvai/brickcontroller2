using BrickController2.HardwareServices.GameController;
using BrickController2.Helpers;

namespace BrickController2.UI.ViewModels
{
    public class ControllerEventViewModel : NotifyPropertyChangedSource
    {
        private float _value;

        public ControllerEventViewModel(GameControllerEventType eventType, string eventCode, float value)
        {
            EventType = eventType;
            EventCode = eventCode;
            Value = value;
        }

        public GameControllerEventType EventType { get; }
        public string EventCode { get; }

        public float Value
        {
            get => _value;
            set { _value = value; RaisePropertyChanged(); }
        }
    }
}
