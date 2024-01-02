using Android.App;
using Android.Runtime;
using Autofac;
using BrickController2.DI;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.Droid.UI.CustomRenderers;
using BrickController2.Droid.UI.Services.DI;
using BrickController2.UI.Controls;

namespace BrickController2.Droid
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => ApplicationBuilder.Create()
            // per platform handlers
            .ConfigureMauiHandlers(handlers =>
            {
                handlers
                    .AddHandler<ExtendedSlider, ExtendedSliderHandler>()
                    .AddHandler<ColorImage, ColorImageHnadler>()
                ;
            })
            .ConfigureContainer((containerBuilder) =>
            {
                containerBuilder.Register<Android.Content.Context>((c) => Android.App.Application.Context).SingleInstance();
                containerBuilder.RegisterModule<PlatformServicesModule>();
                containerBuilder.RegisterModule<UIServicesModule>();
            })
            // finally build
            .Build()
            ;
    }
}
