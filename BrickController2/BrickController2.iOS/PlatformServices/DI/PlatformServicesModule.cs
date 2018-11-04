using Autofac;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.Infrared;
using BrickController2.iOS.PlatformServices.GameController;
using BrickController2.iOS.PlatformServices.Infrared;
using BrickController2.iOS.PlatformServices.Versioning;
using BrickController2.PlatformServices.Versioning;
using BrickController2.iOS.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.iOS.PlatformServices.DI
{
    public class PlatformServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
            builder.RegisterType<GameControllerService>().AsSelf().As<IGameControllerService>().SingleInstance();
            builder.RegisterType<VersionService>().As<IVersionService>().SingleInstance();
            builder.RegisterType<BluetoothLEService>().As<IBluetoothLEService>().SingleInstance();
        }
    }
}