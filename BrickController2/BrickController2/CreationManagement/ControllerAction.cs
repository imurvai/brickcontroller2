using SQLite;
using SQLiteNetExtensions.Attributes;

namespace BrickController2.CreationManagement
{
    public class ControllerAction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ControllerEvent))]
        public int ControllerEventId { get; set; }

        [ManyToOne]
        public ControllerEvent ControllerEvent { get; set; }

        public string DeviceID { get; set; }
        public int Channel { get; set; }
        public bool IsInvert { get; set; }
        public bool IsToggle { get; set; }
        public int MaxOutput { get; set; }
    }
}
