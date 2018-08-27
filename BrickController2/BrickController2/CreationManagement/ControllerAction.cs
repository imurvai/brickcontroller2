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
        private bool _isToggle;
        private int _maxOutput;

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

        public bool IsToggle
        {
            get { return _isToggle; }
            set { _isToggle = value; RaisePropertyChanged(); }
        }

        public int MaxOutput
        {
            get { return _maxOutput; }
            set { _maxOutput = value; RaisePropertyChanged(); }
        }
    }
}
