using Autofac;
using Plugin.BluetoothLE;

namespace BrickController2.DeviceManagement.DI
{
    public class DeviceManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(CrossBleAdapter.Current).As<IAdapter>().SingleInstance();

            builder.RegisterType<BluetoothDeviceManager>().As<IBluetoothDeviceManager>().SingleInstance();
            builder.RegisterType<InfraredDeviceManager>().As<IInfraredDeviceManager>().SingleInstance();

            builder.RegisterType<DeviceRepository>().As<IDeviceRepository>().SingleInstance();
            builder.RegisterType<DeviceManager>().As<IDeviceManager>().SingleInstance();

            builder.RegisterType<SBrickDevice>().Keyed<Device>(DeviceType.SBrick);
            builder.RegisterType<BuWizzDevice>().Keyed<Device>(DeviceType.BuWizz);
            builder.RegisterType<BuWizz2Device>().Keyed<Device>(DeviceType.BuWizz2);
            builder.RegisterType<InfraredDevice>().Keyed<Device>(DeviceType.Infrared);
            builder.RegisterType<PoweredUpDevice>().Keyed<Device>(DeviceType.PoweredUp);

            builder.Register<DeviceFactory>(c =>
            {
                IComponentContext ctx = c.Resolve<IComponentContext>();
                return (deviceType, name, address) => ctx.ResolveKeyed<Device>(deviceType, new NamedParameter("name", name), new NamedParameter("address", address));
            });
        }
    }
}
