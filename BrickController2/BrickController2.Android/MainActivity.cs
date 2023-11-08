using BrickController2.Droid.PlatformServices.GameController;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.Droid.UI.Services.DI;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Autofac;
using IContainer = Autofac.IContainer;
using Android.Content;
using BrickController2.BusinessLogic.DI;
using BrickController2.Database.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.UI.DI;
using Android.Runtime;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity
    {
        private GameControllerService _gameControllerService;

        #region Activity

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //TabLayoutResource = Resource.Layout.Tabbar;
            //ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            //Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            //Platform.Init(this, bundle);

            //Forms.SetFlags("SwipeView_Experimental");
            //Forms.Init(this, bundle);

            var container = InitDI();
            _gameControllerService = container.Resolve<GameControllerService>();

            var app = container.Resolve<App>();
            //TODO LoadApplication(app);
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

