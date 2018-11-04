using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private readonly byte[] _sendBuffer = new byte[] { 0x0a, 0x00, 0x81, 0x00, 0x11, 0x60, 0x00, 0x00, 0x00, 0x00 };
        private readonly int[] _outputValues = new int[2];

        private IGattCharacteristic _characteristic;

        private int _sendAttemptsLeft;

        public PoweredUpDevice(string name, string address, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;

        public override int NumberOfChannels => 2;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(100 * value);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        protected override bool ProcessServices(IEnumerable<IGattService> services)
        {
            var service = services.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            return _characteristic != null;
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        int v0 = _outputValues[0];
                        var result0 = await SendOutputValueAsync(0, v0);

                        int v1 = _outputValues[1];
                        var result1 = await SendOutputValueAsync(1, v1);

                        if (result0 && result1)
                        {
                            if (v0 != 0 || v1 != 0)
                            {
                                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                            }
                            else
                            {
                                _sendAttemptsLeft--;
                            }
                        }
                        else
                        {
                            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task<bool> SendOutputValueAsync(int channel, int value)
        {
            try
            {
                _sendBuffer[3] = (byte)channel;
                _sendBuffer[7] = (byte)(value < 0 ? (255 + value) : value);

                await _bleDevice?.WriteAsync(_characteristic, _sendBuffer);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
