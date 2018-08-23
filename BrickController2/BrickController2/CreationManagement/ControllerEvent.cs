using BrickController2.HardwareServices;
using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class ControllerEvent : NotifyPropertyChangedSource
    {
        private GameControllerEventType _eventType;
        private string _eventCode;
        private ObservableCollection<ControllerAction> _controllerActions;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ControllerProfile))]
        public int ControllerProfileId { get; set; }

        [ManyToOne]
        public ControllerProfile ControllerProfile { get; set; }

        public GameControllerEventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; RaisePropertyChanged(); }
        }

        public string EventCode
        {
            get { return _eventCode; }
            set { _eventCode = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<ControllerAction> ControllerActions
        {
            get { return _controllerActions; }
            set { _controllerActions = value; RaisePropertyChanged(); }
        }
    }
}
