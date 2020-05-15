using Autofac;
using BrickController2.UI.Services.Dialog;

namespace BrickController2.Droid.UI.Services.DI
{
    public class UIServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
        }
    }
}