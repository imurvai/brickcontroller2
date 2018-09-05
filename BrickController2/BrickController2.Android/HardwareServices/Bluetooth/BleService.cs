using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using BrickController2.HardwareServices.Bluetooth;
using BrickController2.Helpers;

namespace BrickController2.Droid.HardwareServices.Bluetooth
{
    public class BleService : IBleService
    {
        private readonly Context _context;
        private readonly BluetoothAdapter _bluetoothAdapter;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public BleService(Context context)
        {
            _context = context;

            BluetoothManager bluetoothManager = null;
            if (_context.PackageManager?.HasSystemFeature(PackageManager.FeatureBluetoothLe) ?? false)
            {
                bluetoothManager = (BluetoothManager)_context.GetSystemService(Context.BluetoothService);
            }

            _bluetoothAdapter = bluetoothManager?.Adapter;
        }

        public bool IsBleSupported => _bluetoothAdapter != null;

        public bool IsBluetoothOn => _bluetoothAdapter.IsEnabled;

        public async Task ScanAsync(Func<IBleDevice, Task> foundDeviceAsync, CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                await OldScanAsync(foundDeviceAsync, token);
            }
        }

        private async Task NewScanAsync(Func<IBleDevice, Task> foundDeviceAsync, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private async Task OldScanAsync(Func<IBleDevice, Task> foundDeviceAsync, CancellationToken token)
        {
            using (var oldScanCallback = new OldScanCallback(
                async (bluetoothDevice, rssi, scanRecord) =>
                {
                    await foundDeviceAsync(new BleDevice(_context, _bluetoothAdapter, bluetoothDevice.Address));
                }))
            {
                try
                {
                    _bluetoothAdapter.StartLeScan(oldScanCallback);
                    await token.WaitAsync();
                }
                catch (Exception)
                {
                }
                finally
                {
                    _bluetoothAdapter.StopLeScan(oldScanCallback);
                }
            }
        }

        private class NewScanCallback : ScanCallback
        {
            private readonly Action<BluetoothDevice, int, byte[]> _action;

            public NewScanCallback(Action<BluetoothDevice, int, byte[]> action)
            {
                _action = action;
            }

            public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
            {
                _action(result.Device, result.Rssi, result.ScanRecord.GetBytes());
            }

            public override void OnBatchScanResults(IList<ScanResult> results)
            {
                foreach (var result in results)
                {
                    _action(result.Device, result.Rssi, result.ScanRecord.GetBytes());
                }
            }
        }

        private class OldScanCallback : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
        {
            private readonly Action<BluetoothDevice, int, byte[]> _action;

            public OldScanCallback(Action<BluetoothDevice, int, byte[]> action)
            {
                _action = action;
            }

            public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
            {
                _action(device, rssi, scanRecord);
            }
        }
    }
}