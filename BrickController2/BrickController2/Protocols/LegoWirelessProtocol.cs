using System;
using System.Buffers.Binary;

namespace BrickController2.Protocols;

/// <summary>
/// Contains implementation of Lego Wireless Protocol <see href="https://lego.github.io/lego-ble-wireless-protocol-docs/"/>
/// Inspired by <see href="https://github.com/toorisrael/LEGO-Porsche-Controller/blob/main/utils/lwp3_definitions.py"/>
/// </summary>
internal static class LegoWirelessProtocol
{
    // TechnicMove hub ports
    public const byte PORT_DRIVE_MOTOR_1 = 0x32;
    public const byte PORT_DRIVE_MOTOR_2 = 0x33;
    public const byte PORT_STEERING_MOTOR = 0x34;
    public const byte PORT_6LEDS = 0x35;
    public const byte PORT_HUB_LED = 0x3F;

    // port modes
    public const byte PORT_MODE_0 = 0x00;
    public const byte PORT_MODE_1 = 0x01;
    public const byte PORT_MODE_2 = 0x02;
    public const byte PORT_MODE_3 = 0x03;
    public const byte PORT_MODE_4 = 0x04;

    // output command
    public const byte PORT_OUTPUT_COMMAND = 0x81;

    public const byte PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT = 0x51;

    // - output / playvm command
    public const byte PORT_PLAYVM = 0x36;

    public const byte PLAYVM_LIGHTS_OFF_OFF = 0x04;
    public const byte PLAYVM_CALIBRATE_STEERING = 0x08;
    public const byte PLAYVM_COMMAND = 0x10;

    // - output / HUB LED colors
    public const byte HUB_LED_MODE_COLOR = 0x00;
    public const byte HUB_LED_MODE_RGB = 0x01;

    public const byte HUB_LED_COLOR_NONE = 0x00;
    public const byte HUB_LED_COLOR_PINK = 0x01;
    public const byte HUB_LED_COLOR_MAGENTA = 0x02;
    public const byte HUB_LED_COLOR_BLUE = 0x03;
    public const byte HUB_LED_COLOR_LIGHT_BLUE = 0x04;
    public const byte HUB_LED_COLOR_CYAN = 0x05;
    public const byte HUB_LED_COLOR_GREEN = 0x06;
    public const byte HUB_LED_COLOR_YELLOW = 0x07;
    public const byte HUB_LED_COLOR_ORANGE = 0x08;
    public const byte HUB_LED_COLOR_RED = 0x09;
    public const byte HUB_LED_COLOR_WHITE = 0xA;

    // input command (single)
    public const byte PORT_INPUT_COMMAND = 0x41;

    public const byte PORT_VALUE_NOTIFICATION_DISABLED = 0x00;
    public const byte PORT_VALUE_NOTIFICATION_ENABLED = 0x01;

    public const byte FEEDBACK_ACTION_NO_ACTION = 0x00;
    public const byte FEEDBACK_ACTION_ACTION_COMPLETION = 0x01;
    public const byte FEEDBACK_ACTION_ACTION_START = 0x10;
    public const byte FEEDBACK_ACTION_BOTH = 0x11;

    // conversion methods
    public static void ToBytes(int value, out byte b0, out byte b1, out byte b2, out byte b3)
    {
        b0 = (byte)(value & 0xff);
        b1 = (byte)((value >> 8) & 0xff);
        b2 = (byte)((value >> 16) & 0xff);
        b3 = (byte)((value >> 24) & 0xff);
    }

    public static byte ToByte(int value) => (byte)(value & 0xFF);

    public static short ToInt16(byte[] value, int startIndex) => ToInt16(value.AsSpan(startIndex));
    public static int ToInt32(byte[] value, int startIndex) => ToInt32(value.AsSpan(startIndex));

    public static short ToInt16(ReadOnlySpan<byte> value) => BinaryPrimitives.ReadInt16LittleEndian(value);
    public static int ToInt32(ReadOnlySpan<byte> value) => BinaryPrimitives.ReadInt32LittleEndian(value);

    // message builders
    public static byte[] BuildPortInputFormatSetup(byte portId, byte portMode, int interval = 2, byte notification = PORT_VALUE_NOTIFICATION_ENABLED)
    {
        // Message Type - Port Input Format Setup (Single) [0x41]
        ToBytes(interval, out var i0, out var i1, out var i2, out var i3);
        return [0x0a, 0x00, PORT_INPUT_COMMAND, portId, portMode, i0, i1, i2, i3, notification];
    }

    public static byte[] BuildPortOutput_LedMask(byte portId, byte portMode, byte ledMask, byte value)
        // Message Type - Port Output Command [0x81] | Write Direct
        => [9, 0x00, PORT_OUTPUT_COMMAND, portId, FEEDBACK_ACTION_BOTH,
            PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, portMode, ledMask, value];

    public static byte[] BuildPortOutput_HubLed(byte portId, byte mode, byte color)
    // Message Type - Port Output Command [0x81] | Write Direct
    => [8, 0x00, PORT_OUTPUT_COMMAND, portId, FEEDBACK_ACTION_BOTH,
            PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, mode, color];

    public static byte[] BuildPortOutput_PlayVm(int speedValue = 0, int servoValue = 0, byte vmCmd = PLAYVM_LIGHTS_OFF_OFF)
    {
        var speedRaw = ToByte(speedValue);
        var steeringRaw = ToByte(servoValue);
        return [13, 0x00, PORT_OUTPUT_COMMAND, PORT_PLAYVM, FEEDBACK_ACTION_BOTH,
                PORT_OUTPUT_SUBCOMMAND_WRITE_DIRECT, PORT_MODE_0, 0x03, 0x00, speedRaw, steeringRaw, vmCmd, 0x00];
    }
}
