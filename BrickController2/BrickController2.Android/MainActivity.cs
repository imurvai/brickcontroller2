using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Autofac;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Droid.PlatformServices.GameController;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.Droid.UI.Services.DI;
using BrickController2.UI.DI;
using BrickController2.BusinessLogic.DI;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private GameControllerService _gameControllerService;

        #region Activity

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            Platform.Init(this, bundle);
            Forms.Init(this, bundle);

            var container = InitDI();
            _gameControllerService = container.Resolve<GameControllerService>();

            var app = container.Resolve<App>();
            LoadApplication(app);
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            return _gameControllerService.OnKeyDown(keyCode, e) || base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            return _gameControllerService.OnKeyUp(keyCode, e) || base.OnKeyUp(keyCode, e);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            return _gameControllerService.OnGenericMotionEvent(e) || base.OnGenericMotionEvent(e);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #endregion

        private IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(this).As<Context>().As<Activity>();
            builder.RegisterModule(new PlatformServicesModule());
            builder.RegisterModule(new UIServicesModule());

            builder.RegisterModule(new BusinessLogicModule());
            builder.RegisterModule(new DatabaseModule());
            builder.RegisterModule(new CreationManagementModule());
            builder.RegisterModule(new DeviceManagementModule());
            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}

