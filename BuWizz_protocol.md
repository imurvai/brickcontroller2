# BuWizz protocol description

## Remark

This protocol description is not official. It was reverse-engineered based on the bluetooth communication logs.

## BuWizz V1

Manufacturer data in scanrecord starts with: 0x48, 0x4D

__Service UUID__: 0000ffe0-0000-1000-8000-00805f9b34fb

__Characteristic UUID__: 0000ffe1-0000-1000-8000-00805f9b34fb

### Payload:

The payload consists of 5 bytes, 4 channel values and an output level byte in a single __write command__ to the characteristic:

|   | 8. bit | 7. bit | 1-6 bits |
| - |:------:|:------:|:--------:|
| Byte 1 | 1 | Channel value sign | 6 bit abs channel 1 value |
| Byte 2 | 0 | Channel value sign | 6 bit abs channel 2 value |
| Byte 3 | 0 | Channel value sign | 6 bit abs channel 3 value |
| Byte 4 | 0 | Channel value sign | 6 bit abs channel 4 value |
| Byte 5 | 0 | 0 | Output level |

The output level values: low: __0x00__, medium: __0x20__, high: __0x40__

## BuWizz V2

Manufacturer data in scanrecord starts with: __0x4E, 0x05, 0x42, 0x57, 0x00__

There are two revisions of BuWizz2 (I'm aware of at least):
- Rev 1 manufacturer data continues with __0x1B__
- Rev 2 manufacturer data contunues with __0x1E__

__Service UUID__: 4e050000-74fb-4481-88b3-9919b1676e93

__Characteristic UUID__: 000092d1-0000-1000-8000-00805f9b34fb

### Payload:

For V2 setting the output level and the output values can be done in separate __write requests__ to the characteristic.

1. Setting the output level:
In this case the payload consist of 2 bytes: __0x11__ and __output level__

The output level values: low: __1__, medium: __2__, high: __3__, ludicrous: __4__

2. Setting the channel outputs:
It consists of 6 bytes:

|  | Value for Rev 1 | Value for Rev 2 |
|-|:-----:|:-----:|
| Byte 1 | 0x10 | 0x10 |
| Byte 2 | 7 bit signed channel 1 output value | 7 bit signed channel 2 output value |
| Byte 3 | 7 bit signed channel 2 output value | 7 bit signed channel 1 output value |
| Byte 4 | 7 bit signed channel 3 output value | 7 bit signed channel 4 output value |
| Byte 5 | 7 bit signed channel 4 output value | 7 bit signed channel 3 output value |
| Byte 6 | 0x00 | 0x00 |
