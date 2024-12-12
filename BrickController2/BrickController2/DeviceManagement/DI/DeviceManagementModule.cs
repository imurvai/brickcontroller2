using Autofac;

namespace BrickController2.DeviceManagement.DI
{
    public class DeviceManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BluetoothDeviceManager>().As<IBluetoothDeviceManager>().SingleInstance();
            builder.RegisterType<InfraredDeviceManager>().As<IInfraredDeviceManager>().SingleInstance();

            builder.RegisterType<DeviceRepository>().As<IDeviceRepository>().SingleInstance();
            builder.RegisterType<DeviceManager>().As<IDeviceManager>().SingleInstance();

            builder.RegisterType<SBrickDevice>().Keyed<Device>(DeviceType.SBrick);
            builder.RegisterType<BuWizzDevice>().Keyed<Device>(DeviceType.BuWizz);
            builder.RegisterType<BuWizz2Device>().Keyed<Device>(DeviceType.BuWizz2);
            builder.RegisterType<BuWizz3Device>().Keyed<Device>(DeviceType.BuWizz3);
            builder.RegisterType<InfraredDevice>().Keyed<Device>(DeviceType.Infrared);
            builder.RegisterType<PoweredUpDevice>().Keyed<Device>(DeviceType.PoweredUp);
            builder.RegisterType<BoostDevice>().Keyed<Device>(DeviceType.Boost);
            builder.RegisterType<TechnicHubDevice>().Keyed<Device>(DeviceType.TechnicHub);
            builder.RegisterType<DuploTrainHubDevice>().Keyed<Device>(DeviceType.DuploTrainHub);
            builder.RegisterType<CircuitCubeDevice>().Keyed<Device>(DeviceType.CircuitCubes);
            builder.RegisterType<Wedo2Device>().Keyed<Device>(DeviceType.WeDo2);
            builder.RegisterType<TechnicMoveDevice>().Keyed<Device>(DeviceType.TechnicMove);

            builder.Register<DeviceFactory>(c =>
            {
                IComponentContext ctx = c.Resolve<IComponentContext>();
                return (deviceType, name, address, deviceData) => ctx.ResolveKeyed<Device>(deviceType, new NamedParameter("name", name), new NamedParameter("address", address), new NamedParameter("deviceData", deviceData));
            });
        }
    }
}
