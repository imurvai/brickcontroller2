using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    public class BluetoothLEService : IBluetoothLEService
    {
        private readonly Context _context;
        private readonly BluetoothAdapter _bluetoothAdapter;

        private bool _isScanning = false;

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

        public async Task<bool> ScanDevicesAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            if (!IsBluetoothLESupported || !IsBluetoothOn || _isScanning)
            {
                return false;
            }

            try
            {
                _isScanning = true;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    return await NewScanAsync(scanCallback, token);
                }
                else
                {
                    return await OldScanAsync(scanCallback, token);
                }
            }
            finally
            {
                _isScanning = false;
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

        private Task<bool> OldScanAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            var leScanner = new BluetoothLEOldScanner(scanCallback);
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

        private Task<bool> NewScanAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            var leScanner = new BluetoothLENewScanner(scanCallback);
            var settingsBuilder = new ScanSettings.Builder()
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);

            _bluetoothAdapter.BluetoothLeScanner.StartScan(null, settingsBuilder.Build(), leScanner);

            var tcs = new TaskCompletionSource<bool>();
            token.Register(() =>
            {
                _bluetoothAdapter.BluetoothLeScanner.StopScan(leScanner);
                tcs.SetResult(true);
            });

            return tcs.Task;
        }
    }
}