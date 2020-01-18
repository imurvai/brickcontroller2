using Autofac;

namespace BrickController2.BusinessLogic.DI
{
    public class BusinessLogicModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PlayLogic>().As<IPlayLogic>().InstancePerDependency();
            builder.RegisterType<SequencePlayer>().As<ISequencePlayer>().InstancePerDependency();
        }
    }
}
