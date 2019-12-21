﻿using BrickController2.Helpers;
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
        private ObservableCollection<SequenceControlPoint> _controlPoints = new ObservableCollection<SequenceControlPoint>();

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
        public ObservableCollection<SequenceControlPoint> ControlPoints
        {
            get { return _controlPoints; }
            set { _controlPoints = value; RaisePropertyChanged(); }
        }

        [Ignore]
        public int TotalDurationMs
        {
            get
            {
                var td = 0;
                foreach (var cp in ControlPoints)
                {
                    td += cp.DurationMs;
                }

                return td;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
