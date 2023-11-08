using Android.App;
using Android.Runtime;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.Droid.UI.Services.DI;
using BrickController2.UI.DI;

namespace BrickController2.Droid
{
#if DEBUG
    [Application(Debuggable = true)]
#else
[Application(Debuggable = false)]
#endif
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            var appBuilder = MauiApp.CreateBuilder();

            appBuilder.ConfigureContainer(new AutofacServiceProviderFactory(), (builder) =>
            {
                //TODO builder.RegisterInstance(this).As<Context>().As<Activity>();
                builder.RegisterModule(new PlatformServicesModule());
                builder.RegisterModule(new UIServicesModule());

                builder.RegisterModule(new BusinessLogicModule());
                builder.RegisterModule(new DatabaseModule());
                builder.RegisterModule(new CreationManagementModule());
                builder.RegisterModule(new DeviceManagementModule());
                builder.RegisterModule(new UiModule());
            });

            return appBuilder.UseMauiApp<App>()
                .Build();
        }
    }
}