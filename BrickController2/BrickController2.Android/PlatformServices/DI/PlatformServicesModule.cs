using Autofac;
using BrickController2.Droid.PlatformServices.GameController;
using BrickController2.Droid.PlatformServices.Infrared;
using BrickController2.Droid.PlatformServices.Versioning;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.Infrared;
using BrickController2.PlatformServices.Versioning;

namespace BrickController2.Droid.PlatformServices.DI
{
    public class PlatformServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
            builder.RegisterType<GameControllerService>().AsSelf().As<IGameControllerService>().SingleInstance();
            builder.RegisterType<VersionService>().As<IVersionService>().SingleInstance();
        }
    }
}