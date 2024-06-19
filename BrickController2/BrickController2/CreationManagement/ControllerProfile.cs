using BrickController2.Helpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class ControllerProfile : NotifyPropertyChangedSource
    {
        private string _name = string.Empty;
        private ObservableCollection<ControllerEvent> _controllerEvents = new ObservableCollection<ControllerEvent>();

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        [ForeignKey(typeof(Creation))]
        [JsonIgnore]
        public int CreationId { get; set; }

        [ManyToOne]
        [JsonIgnore]
        public Creation? Creation { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<ControllerEvent> ControllerEvents
        {
            get { return _controllerEvents; }
            set { _controllerEvents = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
