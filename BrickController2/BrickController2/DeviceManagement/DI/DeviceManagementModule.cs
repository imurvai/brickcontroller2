using Autofac;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace BrickController2.DeviceManagement.DI
{
    public class DeviceManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(CrossBluetoothLE.Current).As<IBluetoothLE>().SingleInstance();
            builder.RegisterInstance(CrossBluetoothLE.Current.Adapter).As<IAdapter>().SingleInstance();

            builder.RegisterType<BluetoothDeviceManager>().As<IBluetoothDeviceManager>().SingleInstance();
            builder.RegisterType<InfraredDeviceManager>().As<IInfraredDeviceManager>().SingleInstance();

            builder.RegisterType<DeviceRepository>().As<IDeviceRepository>().SingleInstance();
            builder.RegisterType<DeviceManager>().As<IDeviceManager>().SingleInstance();
        }
    }
}
