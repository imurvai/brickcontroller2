using Autofac;
using BrickController2.UI.DI;
using Foundation;
using UIKit;

namespace BrickController2.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication uiApp, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            var container = InitDI();
            var app = container.Resolve<App>();
            LoadApplication(app);

            return base.FinishedLaunching(uiApp, options);
        }

        private IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}
