using SQLite;
using System;

namespace BrickController2.DeviceManagement
{
    [Table("Device")]
    internal class DeviceDTO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DeviceType DeviceType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public byte[] DeviceData { get; set; } = Array.Empty<byte>();
    }
}
