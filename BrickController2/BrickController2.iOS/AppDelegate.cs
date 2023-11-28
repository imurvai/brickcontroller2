using Autofac;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Helpers;
using BrickController2.iOS.PlatformServices.DI;
using BrickController2.iOS.UI.CustomRenderers;
using BrickController2.iOS.UI.Services.DI;
using BrickController2.UI.Controls;
using BrickController2.UI.DI;
using Foundation;

namespace BrickController2.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers
                        .AddHandler<ExtendedSlider, ExtendedSliderRenderer>()
                        .AddHandler<ColorImage, ColorImageRenderer>()
                    //TODO NoAnimListViewRenderer
                    ;
                })
                .ConfigureContainer((containerBuilder) =>
                {
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
