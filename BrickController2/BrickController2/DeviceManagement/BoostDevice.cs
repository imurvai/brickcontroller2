﻿using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    internal class BoostDevice : ControlPlusDevice
    {
        public BoostDevice(
            string name,
            string address,
            byte[] deviceData,
            IDeviceRepository deviceRepository,
            IBluetoothLEService bleService)
            : base(name, address, deviceRepository, bleService)
        {
        }

        public override DeviceType DeviceType => DeviceType.Boost;
        public override int NumberOfChannels => 4;

        protected override void RegisterDefaultPorts()
        {
            RegisterPorts(new[]
            {
                new DevicePort(0, "A"),
                new DevicePort(1, "B"),
                new DevicePort(2, "C"),
                new DevicePort(3, "D"),
            });
        }
    }
}
