using Android.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    internal class GattService : IGattService
    {
        public GattService(BluetoothGattService bluetoothGattService, IEnumerable<GattCharacteristic> characteristics)
        {
            BluetoothGattService = bluetoothGattService;
            Characteristics = characteristics;
        }

        public BluetoothGattService BluetoothGattService { get; }
        public Guid Uuid => BluetoothGattService.Uuid.ToGuid();
        public IEnumerable<IGattCharacteristic> Characteristics { get; }
    }
}