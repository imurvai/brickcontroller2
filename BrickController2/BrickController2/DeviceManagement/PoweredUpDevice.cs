using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class PoweredUpDevice : BluetoothDevice
    {
        private const int MAX_SEND_ATTEMPTS = 4;

        private readonly Guid SERVICE_UUID = new Guid("00001623-1212-efde-1623-785feabcd123");
        private readonly Guid CHARACTERISTIC_UUID = new Guid("00001624-1212-efde-1623-785feabcd123");

        private readonly int[] _outputValues = new int[2];
        private int _outputLevelValue;

        private IGattCharacteristic _characteristic;

        private Task _outputTask;
        private CancellationTokenSource _outputTaskTokenSource;
        private object _outputTaskLock = new object();
        private int _sendAttemptsLeft;

        public PoweredUpDevice(string name, string address, IDeviceRepository deviceRepository, IAdapter adapter)
            : base(name, address, deviceRepository, adapter)
        {
        }

        public override DeviceType DeviceType => DeviceType.PoweredUp;

        public override int NumberOfChannels => 2;

        public override void SetOutput(int channel, int value)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<bool> ConnectPostActionAsync(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        protected override Task DisconnectPreActionAsync(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<bool> ServicesDiscovered(IList<IGattService> services, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }
    }
}
