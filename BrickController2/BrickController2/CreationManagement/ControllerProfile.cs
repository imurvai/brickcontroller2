﻿using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class ControllerProfile : NotifyPropertyChangedSource
    {
        private string _name;
        private ObservableCollection<ControllerEvent> _controllerEvents = new ObservableCollection<ControllerEvent>();

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
        public ObservableCollection<ControllerEvent> ControllerEvents
        {
            get { return _controllerEvents; }
            set { _controllerEvents = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }

        public ControllerProfile Clone(string newName)
        {
            // create new instance as a copy including events
            var copy = new ControllerProfile
            {
                Name = newName
            };
            if (_controllerEvents != null)
            {
                foreach (var controllerEvent in _controllerEvents)
                {
                    copy.ControllerEvents.Add(controllerEvent.Clone());
                }
            }

            return copy;
        }
    }
}
