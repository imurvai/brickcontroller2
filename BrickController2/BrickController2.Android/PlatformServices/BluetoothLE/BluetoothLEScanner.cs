using System;
using Android.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEScanner : BluetoothAdapter.ILeScanCallback
    {
        private readonly Action<ScanResult> _scanCallback;

        public BluetoothLEScanner(Action<ScanResult> scanCallback)
        {
            _scanCallback = scanCallback;
        }

        public IntPtr Handle => throw new NotImplementedException();

        public void Dispose()
        {
        }

        public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
        {
            _scanCallback(new ScanResult(device.Name, device.Address, scanRecord));
        }
    }
}