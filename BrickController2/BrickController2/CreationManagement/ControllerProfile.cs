using SQLite;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace BrickController2.CreationManagement
{
    public class ControllerProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(Creation))]
        public int CreationId { get; set; }

        [ManyToOne]
        public Creation Creation { get; set; }

        public string Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<ControllerEvent> ControllerEvents { get; set; }
    }
}
