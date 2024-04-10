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
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _isScanning = false;
            }
        }

        public IBluetoothLEDevice GetKnownDevice(string address)
        {
            if (!IsBluetoothLESupported)
            {
                return null;
            }

            return new BluetoothLEDevice(_context, _bluetoothAdapter, address);
        }

        private async Task<bool> OldScanAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            try
            {
                var leScanner = new BluetoothLEOldScanner(scanCallback);
#pragma warning disable CS0618 // Type or member is obsolete
                if (!_bluetoothAdapter.StartLeScan(leScanner))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    return false;
                }

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (token.Register(() =>
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    _bluetoothAdapter.StopLeScan(leScanner);
#pragma warning restore CS0618 // Type or member is obsolete
                    tcs.TrySetResult(true);
                }))
                {
                    return await tcs.Task;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> NewScanAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
        {
            try
            {
                var leScanner = new BluetoothLENewScanner(scanCallback);
                var settingsBuilder = new ScanSettings.Builder()
                    .SetCallbackType(ScanCallbackType.AllMatches)
                    .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);

                _bluetoothAdapter.BluetoothLeScanner.StartScan(null, settingsBuilder.Build(), leScanner);

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                using (token.Register(() =>
                {
                    _bluetoothAdapter.BluetoothLeScanner.StopScan(leScanner);
                    tcs.TrySetResult(true);
                }))
                {
                    return await tcs.Task;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}