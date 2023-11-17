using Android.App;
using Android.Runtime;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.Droid.UI.CustomRenderers;
using BrickController2.Droid.UI.Services.DI;
using BrickController2.UI.Controls;
using BrickController2.UI.DI;

namespace BrickController2.Droid
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers
                        .AddHandler(typeof(ExtendedSlider), typeof(ExtendedSliderRenderer))
                        .AddHandler(typeof(ColorImage), typeof(ColorImageRenderer));
                    ;
                })
                .ConfigureContainer(new AutofacServiceProviderFactory(), (containerBuilder) =>
                {
                    containerBuilder.Register<Android.Content.Context>((c) => Android.App.Application.Context).SingleInstance();
                    containerBuilder.RegisterModule<PlatformServicesModule>();
                    containerBuilder.RegisterModule<UIServicesModule>();

                    containerBuilder.RegisterModule<BusinessLogicModule>();
                    containerBuilder.RegisterModule<DatabaseModule>();
                    containerBuilder.RegisterModule<CreationManagementModule>();
                    containerBuilder.RegisterModule<DeviceManagementModule>();
                    containerBuilder.RegisterModule<UiModule>();
                })
                ;
#if DEBUG
            //builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}