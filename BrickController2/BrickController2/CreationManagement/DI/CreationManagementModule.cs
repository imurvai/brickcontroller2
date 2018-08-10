using Autofac;

namespace BrickController2.CreationManagement.DI
{
    public class CreationManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CreationRepository>().As<ICreationRepository>().SingleInstance();
        }
    }
}
