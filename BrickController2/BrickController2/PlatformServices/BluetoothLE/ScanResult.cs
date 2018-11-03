using System.Collections.Generic;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public class ScanResult
    {
        public ScanResult(string deviceName, string deviceAddress, IDictionary<byte, byte[]> advertismentData)
        {
            DeviceName = deviceName;
            DeviceAddress = deviceAddress;
            AdvertismentData = advertismentData;
        }

        public string DeviceName { get; }
        public string DeviceAddress { get; }
        public IDictionary<byte, byte[]> AdvertismentData { get; }
    }
}
