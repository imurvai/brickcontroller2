namespace BrickController2.PlatformServices.BluetoothLE
{
    public class ScanResult
    {
        public ScanResult(string deviceName, string deviceAddress, byte[] scanRecord)
        {
            DeviceName = deviceName;
            DeviceAddress = deviceAddress;
            ScanRecord = scanRecord;
        }

        public string DeviceName { get; }
        public string DeviceAddress { get; }
        public byte[] ScanRecord { get; }
    }
}
