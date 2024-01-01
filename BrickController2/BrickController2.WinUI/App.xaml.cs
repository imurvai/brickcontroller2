using Autofac;
using BrickController2.DI;
using BrickController2.UI.Controls;
using BrickController2.Windows.PlatformServices.DI;
using BrickController2.Windows.UI.CustomHandlers;
using Windows.Devices.Input;

namespace BrickController2.Windows
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            InitializeComponent();
        }
        protected override MauiApp CreateMauiApp() => ApplicationBuilder.Create()
            // per platform handlers
            .ConfigureMauiHandlers(handlers =>
            {
                handlers
                    .AddHandler<ExtendedSlider, ExtendedSliderHandler>()
                ;

                // handle swipe if there is no touch screen
                var capablitities = new TouchCapabilities();
                if (capablitities.TouchPresent == 0)
                {
                    handlers.AddHandler<SwipeView, CustomSwipeViewHandler>();
                }
            })
            .ConfigureContainer((containerBuilder) =>
            {
                containerBuilder.RegisterModule<PlatformServicesModule>();
            })
            // finally build
            .Build()
            ;
    }
}
