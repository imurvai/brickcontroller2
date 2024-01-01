using BrickController2.PlatformServices.BluetoothLE;
using Windows.Devices.Bluetooth;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

public class BleService : IBluetoothLEService
{
    [Flags]
    private enum BluetoothStatus
    {
        None = 0x00,
        ClassicSupported = 0x01,
        LowEnergySupported = 0x02,

        AllFeatures = ClassicSupported | LowEnergySupported
    }

    private bool _isScanning;

    public BleService()
    {
    }

    public bool IsBluetoothLESupported => CurrentBluetoothStatus.HasFlag(BluetoothStatus.LowEnergySupported);
    public bool IsBluetoothOn => CurrentBluetoothStatus.HasFlag(BluetoothStatus.ClassicSupported);

    private BluetoothStatus CurrentBluetoothStatus
    {
        get
        {
            // synchroniously wait
            var adapterTask = GetBluetoothAdapter();
            adapterTask.Wait();

            BluetoothStatus status = (adapterTask.Result?.IsClassicSupported ?? false) ? BluetoothStatus.ClassicSupported : BluetoothStatus.None;
            status |= (adapterTask.Result?.IsLowEnergySupported ?? false) ? BluetoothStatus.LowEnergySupported : BluetoothStatus.None;

            return status;
        }
    }

    private static async Task<BluetoothAdapter> GetBluetoothAdapter() => await BluetoothAdapter.GetDefaultAsync()
        .AsTask()
        .ConfigureAwait(false);

    public async Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
    {
        if (_isScanning || CurrentBluetoothStatus != BluetoothStatus.AllFeatures)
        {
            return false;
        }

        try
        {
            _isScanning = true;
            return await NewScanAsync(scanCallback, token);                
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

        return new BleDevice(address);
    }

    private async Task<bool> NewScanAsync(Action<ScanResult> scanCallback, CancellationToken token)
    {
        try
        {
            var leScanner = new BleScanner(scanCallback);

            leScanner.Start();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            token.Register(() =>
            {
                leScanner.Stop();
                tcs.SetResult(true);
            });

            return await tcs.Task;
        }
        catch (Exception)
        {
            return false;
        }
    }
}