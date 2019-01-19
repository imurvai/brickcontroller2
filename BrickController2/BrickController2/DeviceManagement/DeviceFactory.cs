namespace BrickController2.DeviceManagement
{
    internal delegate Device DeviceFactory(DeviceType deviceType, string name, string address, byte[] deviceData);
}
