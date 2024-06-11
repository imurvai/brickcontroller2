using BrickController2.Helpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public class Creation : NotifyPropertyChangedSource
    {
        private string _name = string.Empty;
        private ObservableCollection<ControllerProfile> _controllerProfiles = new ObservableCollection<ControllerProfile>();

        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public ObservableCollection<ControllerProfile> ControllerProfiles
        {
            get { return _controllerProfiles; }
            set { _controllerProfiles = value; RaisePropertyChanged(); }
        }

        public override string ToString()
        {
            return Name;
        }

        public IEnumerable<string> GetDeviceIds()
        {
            var deviceIds = new List<string>();

            foreach (var profile in ControllerProfiles)
            {
                foreach (var controllerEvent in profile.ControllerEvents)
                {
                    foreach (var controllerAction in controllerEvent.ControllerActions)
                    {
                        var deviceId = controllerAction.DeviceId;
                        if (!deviceIds.Contains(deviceId))
                        {
                            deviceIds.Add(deviceId);
                        }
                    }
                }
            }

            return deviceIds;
        }

        public IEnumerable<string> GetSequenceNames()
        {
            var sequenceNames = new List<string>();

            foreach (var profile in ControllerProfiles)
            {
                foreach (var controllerEvent in profile.ControllerEvents)
                {
                    foreach (var controllerAction in controllerEvent.ControllerActions)
                    {
                        if (controllerAction.ButtonType == ControllerButtonType.Sequence)
                        {
                            var sequenceName = controllerAction.SequenceName;
                            if (!sequenceNames.Contains(sequenceName))
                            {
                                sequenceNames.Add(sequenceName);
                            }
                        }
                    }
                }
            }

            return sequenceNames;
        }
    }
}
