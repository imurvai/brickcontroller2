using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Autofac;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Droid.HardwareServices;
using BrickController2.Droid.HardwareServices.DI;
using BrickController2.UI.DI;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private GameControllerService _gameControllerService;

        #region Activity

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

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

        #endregion

        private IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new HardwareServicesModule());

            builder.RegisterModule(new DatabaseModule());
            builder.RegisterModule(new CreationManagementModule());
            builder.RegisterModule(new DeviceManagementModule());
            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}

