using BrickController2.Helpers;
using SQLite;

namespace BrickController2.CreationManagement
{
    public class SequenceControlPoint : NotifyPropertyChangedSource
    {
        private float _value;
        private int _durationMs;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public float Value
        {
            get { return _value; }
            set { _value = value; RaisePropertyChanged(); }
        }

        public int DurationMs
        {
            get { return _durationMs; }
            set { _durationMs = value; RaisePropertyChanged(); }
        }
    }
}
