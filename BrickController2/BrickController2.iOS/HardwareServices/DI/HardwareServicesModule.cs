using Autofac;
using BrickController2.HardwareServices;

namespace BrickController2.iOS.HardwareServices.DI
{
    public class HardwareServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
            builder.RegisterType<GameControllerService>().AsSelf().As<IGameControllerService>().SingleInstance();
        }
    }
}