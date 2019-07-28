using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.UIThread;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class TechnicHubDevice : ControlPlusDevice
    {
        public TechnicHubDevice(
            string name, 
            string address, 
            byte[] deviceData, 
            IDeviceRepository deviceRepository, 
            IUIThreadService uiThreadService, 
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, uiThreadService, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.TechnicHub;
        public override int NumberOfChannels => 4;

        protected override async Task ProcessOutputsAsync(CancellationToken token)
        {
            _outputValues[0] = 0;
            _outputValues[1] = 0;
            _outputValues[2] = 0;
            _outputValues[3] = 0;
            _lastOutputValues[0] = 1;
            _lastOutputValues[1] = 1;
            _lastOutputValues[2] = 1;
            _lastOutputValues[3] = 1;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

            bool isVirtualPortSetup = true;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_sendAttemptsLeft > 0)
                    {
                        if (isVirtualPortSetup)
                        {
                            if (await SetupVirtualPortAsync(0, 1))
                            {
                                isVirtualPortSetup = false;
                            }
                        }
                        else
                        {
                            if (await SendOutputValuesAsync())
                            {
                                int v0 = _outputValues[0];
                                int v1 = _outputValues[1];
                                int v2 = _outputValues[2];
                                int v3 = _outputValues[3];

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
                int v2 = _outputValues[2];
                int v3 = _outputValues[3];
                int msa0 = _maxServoAngles[0];
                int msa1 = _maxServoAngles[1];
                int msa2 = _maxServoAngles[2];
                int msa3 = _maxServoAngles[3];

                var result = true;

                if (msa0 < 0 && msa1 < 0)
                {
                    result = result && await SendOutputValueVirtualAsync(0x10, 0, 1, v0, v1);
                }
                else
                {
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
                }

                if (msa2 < 0)
                {
                    result = result && await SendOutputValueAsync(2, v2);
                }
                else
                {
                    result = result && await SendServoOutputValueAsync(2, v2, msa2);
                }

                if (msa3 < 0)
                {
                    result = result && await SendOutputValueAsync(3, v3);
                }
                else
                {
                    result = result && await SendServoOutputValueAsync(3, v3, msa3);
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
