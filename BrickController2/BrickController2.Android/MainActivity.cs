using BrickController2.Droid.PlatformServices.GameController;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Runtime;
using AndroidX.AppCompat.App;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : AppCompatActivity
    {
        private readonly GameControllerService _gameControllerService;

        public MainActivity()
        {
            // inject the controller
            _gameControllerService = MauiApplication.Current.Services.GetRequiredService<GameControllerService>();
        }

        #region Activity


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
    }
}

