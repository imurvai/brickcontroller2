namespace BrickController2.DeviceManagement
{
    internal abstract class BluetoothDevice : Device
    {
        public BluetoothDevice(string name, string address)
            : base(name, address)
        {
        }
    }
}
