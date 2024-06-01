using System;
using Android.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEOldScanner : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
    {
        private readonly Action<ScanResult> _scanCallback;

        public BluetoothLEOldScanner(Action<ScanResult> scanCallback)
        {
            _scanCallback = scanCallback;
        }

        public void OnLeScan(BluetoothDevice? device, int rssi, byte[]? scanRecord)
        {
            if (device is null ||
                string.IsNullOrEmpty(device.Name) ||
                string.IsNullOrEmpty(device.Address) ||
                scanRecord is null)
            {
                return;
            }

            var advertismentData = ScanRecordProcessor.GetAdvertismentData(scanRecord);
            _scanCallback(new ScanResult(device.Name, device.Address, advertismentData));
        }
    }
}