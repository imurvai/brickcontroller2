using Autofac;
using BrickController2.Droid.HardwareServices.GameController;
using BrickController2.Droid.HardwareServices.Infrared;
using BrickController2.HardwareServices.GameController;
using BrickController2.HardwareServices.Infrared;

namespace BrickController2.Droid.HardwareServices.DI
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