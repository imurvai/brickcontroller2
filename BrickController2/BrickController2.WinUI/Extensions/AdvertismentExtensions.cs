using Windows.Devices.Bluetooth.Advertisement;

namespace BrickController2.Windows.Extensions;

public static class AdvertismentExtensions
{
    public static string GetLocalName(this BluetoothLEAdvertisementReceivedEventArgs args)
    {
        return args.Advertisement.LocalName.TrimEnd();
    }

    public static bool IsValidDeviceName(this string deviceName)
    {
        return !string.IsNullOrEmpty(deviceName);
    }

    public static bool CanCarryData(this BluetoothLEAdvertisementReceivedEventArgs args)
    {
        return args.AdvertisementType == BluetoothLEAdvertisementType.ScanResponse ||
            args.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected;
    }
}
