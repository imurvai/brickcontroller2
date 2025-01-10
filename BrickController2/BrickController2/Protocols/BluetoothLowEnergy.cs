namespace BrickController2.Protocols;

public static class BluetoothLowEnergy
{
    // advertisment data types
    public const byte ADTYPE_SERVICE_128BIT = 0x06;        // Service: Additional 128-bit UUIDs
    public const byte ADTYPE_LOCAL_NAME_COMPLETE = 0x09;   // Complete local name
    public const byte ADTYPE_MANUFACTURER_SPECIFIC = 0xFF; // Manufacturer specific data
}
