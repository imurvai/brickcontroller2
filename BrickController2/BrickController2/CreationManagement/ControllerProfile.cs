using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class ControllerProfile : NotifyPropertyChangedSource
    {
        private string _name;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Creation))]
        public int CreationId { get; set; }

        [ManyToOne]
        public Creation Creation { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<ControllerEvent> ControllerEvents { get; set; }
    }
}
