using BrickController2.Settings;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.ObjectModel;

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

        [TextBlob(nameof(SettingsBlobed))]
        public ObservableCollection<NamedSetting> Settings { get; set; } = [];

        public string? SettingsBlobed { get; set; }
    }
}
