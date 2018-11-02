using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEService : IBluetoothLEService
    {
        private readonly Context _context;
        private readonly BluetoothAdapter _bluetoothAdapter;

        public BluetoothLEService(Context context)
        {
            _context = context;

            if (context.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
            {
                var bluetoothManager = (BluetoothManager)context.GetSystemService(Context.BluetoothService);
                _bluetoothAdapter = bluetoothManager?.Adapter;
            }
            else
            {
                _bluetoothAdapter = null;
            }
        }

        public bool IsBluetoothLESupported => _bluetoothAdapter != null;
        public bool IsBluetoothOn => _bluetoothAdapter?.IsEnabled ?? false;

        public Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
        {
            if (!IsBluetoothLESupported || !IsBluetoothOn)
            {
                return Task.FromResult(false);
            }

            using (var leScanner = new BluetoothLEScanner(scanCallback))
            {
                if (!_bluetoothAdapter.StartLeScan(leScanner))
                {
                    return Task.FromResult(false);
                }

                var tcs = new TaskCompletionSource<bool>();

                token.Register(() =>
                {
                    _bluetoothAdapter.StopLeScan(leScanner);
                    tcs.SetResult(true);
                });

                return tcs.Task;
            }
        }

        public IBluetoothLEDevice GetKnownDevice(string address)
        {
            var device = _bluetoothAdapter?.GetRemoteDevice(address);
            if (device == null)
            {
                return null;
            }

            return new BluetoothLEDevice(_context, device);
        }
    }
}