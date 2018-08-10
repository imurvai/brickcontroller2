using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BrickController2.CreationManagement
{
    public class ControllerEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ControllerProfile))]
        public int ControllerProfileId { get; set; }

        [ManyToOne]
        public ControllerProfile ControllerProfile { get; set; }

        public ControllerEventType EventType { get; set; }
        public string EventCode { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ControllerAction> ControllerActions { get; set; }
    }
}
