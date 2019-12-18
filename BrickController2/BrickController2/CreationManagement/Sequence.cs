using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class Sequence : NotifyPropertyChangedSource
    {
        private string _name;
        private bool _loop;
        private bool _interpolate;
        private ObservableCollection<ControlPoint> _controlPoints = new ObservableCollection<ControlPoint>();

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        public bool Loop
        {
            get { return _loop; }
            set { _loop = value; RaisePropertyChanged(); }
        }

        public bool Interpolate
        {
            get { return _interpolate; }
            set { _interpolate = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        private ObservableCollection<ControlPoint> ControlPoints
        {
            get { return _controlPoints; }
            set { _controlPoints = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class ControlPoint : NotifyPropertyChangedSource
    {
        private float _value;
        private TimeSpan _duration;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public float Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { _duration = value; RaisePropertyChanged(); }
        }
    }
}
