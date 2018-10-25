using Autofac;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.iOS.PlatformServices.DI;
using BrickController2.iOS.UI.Services.DI;
using BrickController2.UI.DI;
using Foundation;
using UIKit;

namespace BrickController2.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApp, NSDictionary options)
        {
            Xamarin.Forms.Forms.Init();

            // Preventing screen turning off
            UIApplication.SharedApplication.IdleTimerDisabled = true;

            var container = InitDI();
            var app = container.Resolve<App>();
            LoadApplication(app);

            return base.FinishedLaunching(uiApp, options);
        }

        private IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PlatformServicesModule());
            builder.RegisterModule(new UIServicesModule());

            builder.RegisterModule(new DatabaseModule());
            builder.RegisterModule(new CreationManagementModule());
            builder.RegisterModule(new DeviceManagementModule());
            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}
