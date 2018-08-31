using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BrickController2.CreationManagement
{
    public class ControllerAction : NotifyPropertyChangedSource
    {
        private string _deviceId;
        private int _channel;
        private bool _isInvert;
        private ControllerButtonType _buttonType;
        private ControllerAxisCharacteristic _axisCharacteristic;
        private int _maxOutputPercent;
        private int _axisDeadZonePercent;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ControllerEvent))]
        public int ControllerEventId { get; set; }

        [ManyToOne]
        public ControllerEvent ControllerEvent { get; set; }

        public string DeviceId
        {
            get { return _deviceId; }
            set { _deviceId = value; RaisePropertyChanged(); }
        }

        public int Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        public bool IsInvert
        {
            get { return _isInvert; }
            set { _isInvert = value; RaisePropertyChanged(); }
        }

        public ControllerButtonType ButtonType
        {
            get { return _buttonType; }
            set { _buttonType = value; RaisePropertyChanged(); }
        }

        public ControllerAxisCharacteristic AxisCharacteristic
        {
            get { return _axisCharacteristic; }
            set { _axisCharacteristic = value; RaisePropertyChanged(); }
        }

        public int MaxOutputPercent
        {
            get { return _maxOutputPercent; }
            set { _maxOutputPercent = value; RaisePropertyChanged(); }
        }

        public int AxisDeadZonePercent
        {
            get { return _axisDeadZonePercent; }
            set { _axisDeadZonePercent = value; RaisePropertyChanged(); }
        }
    }
}
