namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// Represents immutable description of a device port
    /// </summary>
    public class DevicePort
    {
        public DevicePort(int channel, string name)
        {
            Channel = channel;
            Name = name;
        }

        public int Channel { get; }

        public string Name { get; }
    }
}
