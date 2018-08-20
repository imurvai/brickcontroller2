namespace BrickController2.DeviceManagement
{
    public abstract class BluetoothDevice : Device
    {
        public BluetoothDevice(string name, string address)
            : base(name, address)
        {
        }
    }
}
