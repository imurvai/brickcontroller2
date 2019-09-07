using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : ControlPlusDevice
    {
        public PoweredUpDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;
        public override int NumberOfChannels => 2;

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _lastOutputValues[0] = 1;
            _lastOutputValues[1] = 1;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        if (await SendOutputValuesAsync())
                        {
                            int v0 = _outputValues[0];
                            int v1 = _outputValues[1];

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
                    else
                    {
                        await Task.Delay(10);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private async Task<bool> SendOutputValuesAsync()
        {
            try
            {
                int v0 = _outputValues[0];
                int v1 = _outputValues[1];
                int msa0 = _maxServoAngles[0];
                int msa1 = _maxServoAngles[1];

                var result = true;

                if (msa0 < 0)
                {
                    result = result && await SendOutputValueAsync(0, v0);
                }
                else
                {
                    result = result && await SendServoOutputValueAsync(0, v0, msa0);
                }

                if (msa1 < 0)
                {
                    result = result && await SendOutputValueAsync(1, v1);
                }
                else
                {
                    result = result && await SendServoOutputValueAsync(1, v1, msa1);
                }

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
