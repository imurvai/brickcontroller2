namespace BrickController2.DeviceManagement
{
    internal interface IBluetoothDeviceManager : IDeviceScanner
    {
        bool IsBluetoothLESupported { get; }
        bool IsBluetoothOn { get; }
    }
}
