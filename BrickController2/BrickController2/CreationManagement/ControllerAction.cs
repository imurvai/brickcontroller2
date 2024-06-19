using BrickController2.Helpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BrickController2.CreationManagement
{
    public class ControllerAction : NotifyPropertyChangedSource
    {
        private string _deviceId = string.Empty;
        private int _channel;
        private ChannelOutputType _channelOutputType;
        private bool _isInvert;
        private ControllerButtonType _buttonType;
        private ControllerAxisType _axisType;
        private ControllerAxisCharacteristic _axisCharacteristic;
        private int _maxOutputPercent;
        private int _axisActiveZonePercent = 100;
        private int _axisDeadZonePercent;
        private int _maxServoAngle;
        private int _servoBaseAngle;
        private int _stepperAngle;
        private string _sequenceName = string.Empty;

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [ForeignKey(typeof(ControllerEvent))]
        [JsonIgnore]
        public int ControllerEventId { get; set; }

        [ManyToOne]
        [JsonIgnore]
        public ControllerEvent? ControllerEvent { get; set; }

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

        public ChannelOutputType ChannelOutputType
        {
            get { return _channelOutputType; }
            set { _channelOutputType = value; RaisePropertyChanged(); }
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

        public ControllerAxisType AxisType
        {
            get { return _axisType; }
            set { _axisType = value; RaisePropertyChanged(); }
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

        public int AxisActiveZonePercent
        {
            get { return _axisActiveZonePercent; }
            set { _axisActiveZonePercent = value != 0 ? value : 100; RaisePropertyChanged(); }
        }

        public int AxisDeadZonePercent
        {
            get { return _axisDeadZonePercent; }
            set { _axisDeadZonePercent = value; RaisePropertyChanged(); }
        }

        public int MaxServoAngle
        {
            get { return _maxServoAngle; }
            set { _maxServoAngle = value; RaisePropertyChanged(); }
        }

        public int ServoBaseAngle
        {
            get { return _servoBaseAngle; }
            set { _servoBaseAngle = value; RaisePropertyChanged(); }
        }

        public int StepperAngle
        {
            get { return _stepperAngle; }
            set { _stepperAngle = value; RaisePropertyChanged(); }
        }

        public string SequenceName
        {
            get { return _sequenceName; }
            set { _sequenceName = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return $"{DeviceId} - {Channel}";
        }
    }
}
