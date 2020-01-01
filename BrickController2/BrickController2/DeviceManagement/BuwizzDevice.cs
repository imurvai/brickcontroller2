﻿using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class BuWizzDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID = new Guid("0000ffe0-0000-1000-8000-00805f9b34fb");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("0000ffe1-0000-1000-8000-00805f9b34fb");

        private readonly VolatileBuffer<int> _outputValues = new VolatileBuffer<int>(4);

        private volatile int _outputLevelValue;
        private volatile int _sendAttemptsLeft;

        private IGattCharacteristic _characteristic;

        public BuWizzDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.BuWizz;
        public override int NumberOfChannels => 4;
        public override int NumberOfOutputLevels => 3;
        public override int DefaultOutputLevel => 1;
        protected override bool AutoConnectOnFirstConnect => false;

        public override void SetOutput(int channel, float value)
        {
            CheckChannel(channel);
            value = CutOutputValue(value);

            var intValue = (int)(value * 255);
            if (_outputValues[channel] == intValue)
            {
                return;
            }

            _outputValues[channel] = intValue;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }

        public override bool CanSetOutputLevel => true;

        public override void SetOutputLevel(int value)
        {
            _outputLevelValue = Math.Max(0, Math.Min(NumberOfOutputLevels - 1, value));
        }

        protected override Task<bool> ValidateServicesAsync(IEnumerable<IGattService> services, CancellationToken token)
        {
            var service = services?.FirstOrDefault(s => s.Uuid == SERVICE_UUID);
            _characteristic = service?.Characteristics?.FirstOrDefault(c => c.Uuid == CHARACTERISTIC_UUID);

            return Task.FromResult(_characteristic != null);
        }

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _outputValues[2] = 0;
            _outputValues[3] = 0;
            _outputLevelValue = DefaultOutputLevel;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        int v0 = _outputValues[0];
                        int v1 = _outputValues[1];
                        int v2 = _outputValues[2];
                        int v3 = _outputValues[3];

                        if (await SendOutputValuesAsync(v0, v1, v2, v3, token))
                        {
                            if (v0 != 0 || v1 != 0 || v2 != 0 || v3 != 0)
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
                    else
                    {
                        await Task.Delay(2, token);
                    }
                }
                catch
                {
                }
            }
        }

        private async Task<bool> SendOutputValuesAsync(int v0, int v1, int v2, int v3, CancellationToken token)
        {
            try
            {
                var sendBuffer = new byte[]
                {
                    (byte)((Math.Abs(v0) >> 2) | (v0 < 0 ? 0x40 : 0) | 0x80),
                    (byte)((Math.Abs(v1) >> 2) | (v1 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v2) >> 2) | (v2 < 0 ? 0x40 : 0)),
                    (byte)((Math.Abs(v3) >> 2) | (v3 < 0 ? 0x40 : 0)),
                    (byte)(_outputLevelValue * 0x20)
                };

                var result = await _bleDevice?.WriteNoResponseAsync(_characteristic, sendBuffer, token);
                await Task.Delay(60, token);
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}