using Autofac;
using BrickController2.DI;
using BrickController2.iOS.PlatformServices.DI;
using BrickController2.iOS.UI.CustomRenderers;
using BrickController2.iOS.UI.Services.DI;
using BrickController2.UI.Controls;
using Foundation;

namespace BrickController2.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => ApplicationBuilder.Create()
            // per platform handlers
            .ConfigureMauiHandlers(handlers =>
            {
                handlers
                    .AddHandler<ExtendedSlider, ExtendedSliderRenderer>()
                    .AddHandler<ColorImage, ColorImageRenderer>()
                    .AddHandler< ListView, NoAnimListViewRenderer>()
                ;
            })
            .ConfigureContainer((containerBuilder) =>
            {
                containerBuilder.RegisterModule<PlatformServicesModule>();
                containerBuilder.RegisterModule<UIServicesModule>();
            })
            // finally build
            .Build()
            ;
    }
}
