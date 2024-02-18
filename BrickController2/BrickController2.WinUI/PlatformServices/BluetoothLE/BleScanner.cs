using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Windows.Extensions;
using System.Collections.Concurrent;
using System.Text;
using Windows.Devices.Bluetooth.Advertisement;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

public class BleScanner
{
    private readonly Action<ScanResult> _scanCallback;
    private readonly ConcurrentDictionary<ulong, string> _deviceNameCache;

    private readonly BluetoothLEAdvertisementWatcher _passiveWatcher;
    private readonly BluetoothLEAdvertisementWatcher _activeWatcher;

    private static readonly HashSet<byte> AdvertismentDataTypes = new HashSet<byte>(new[]
    {
        BluetoothLEAdvertisementDataTypes.ManufacturerSpecificData,
        BluetoothLEAdvertisementDataTypes.IncompleteService128BitUuids,
        BluetoothLEAdvertisementDataTypes.CompleteLocalName
    });

    public BleScanner(Action<ScanResult> scanCallback)
    {
        _scanCallback = scanCallback;
        _deviceNameCache = new ConcurrentDictionary<ulong, string>();

        // use passive advertisment for name resolution
        _passiveWatcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Passive };

        _passiveWatcher.Received += _passiveWatcher_Received;
        _passiveWatcher.Stopped += _passiveWatcher_Stopped;

        // use active scanner as ScanResult advertisment processor
        // because SBrick contains large manufacture data which may not come in single packet with device name
        _activeWatcher = new BluetoothLEAdvertisementWatcher { ScanningMode = BluetoothLEScanningMode.Active };
        _activeWatcher.Received += _activeWatcher_Received;
    }

    public void Start()
    {
        _passiveWatcher.Start();
        _activeWatcher.Start();
    }

    private void _passiveWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        // simply update device name cache - if valid
        var deviceName = args.GetLocalName();
        if (deviceName.IsValidDeviceName())
        {
            _deviceNameCache.AddOrUpdate(args.BluetoothAddress, deviceName, (key, oldValue) => deviceName);
        }
    }

    private void _passiveWatcher_Stopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args)
    {
        _deviceNameCache.Clear();
    }

    private void _activeWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        if (!args.CanCarryData())
        {
            return;
        }
        // prefer local name if set, otherwise use cache (where only valid names can be)
        string deviceName = args.GetLocalName();
        if (!deviceName.IsValidDeviceName() && !_deviceNameCache.TryGetValue(args.BluetoothAddress, out deviceName))
        {
            return;
        }

        var bluetoothAddress = args.BluetoothAddress.ToBluetoothAddressString();

        var advertismentData = args.Advertisement.DataSections
            .Where(s => AdvertismentDataTypes.Contains(s.DataType))
            .ToDictionary(s => s.DataType, s => s.Data.ToByteArray());

        // enrich data with name manually (SBrick do not like CompleteLocalName, but Buwizz3 requires it)
        if (!advertismentData.ContainsKey(BluetoothLEAdvertisementDataTypes.CompleteLocalName))
        {
            advertismentData[BluetoothLEAdvertisementDataTypes.CompleteLocalName] = Encoding.ASCII.GetBytes(deviceName);
        }

        _scanCallback(new ScanResult(deviceName, bluetoothAddress, advertismentData));
    }

    public void Stop()
    {
        _passiveWatcher.Stop();
        _activeWatcher.Stop();
    }
}