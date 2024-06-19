using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using BrickController2.Droid.PlatformServices.GameController;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = 
            ConfigChanges.ScreenSize | 
            ConfigChanges.Orientation | 
            ConfigChanges.UiMode | 
            ConfigChanges.ScreenLayout | 
            ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity
    {
        private GameControllerService _gameControllerService;

        public MainActivity()
        {
            _gameControllerService = IPlatformApplication.Current!.Services.GetRequiredService<GameControllerService>()!;
        }

        protected override void OnCreate(Bundle? bundle)
        {
            base.OnCreate(bundle);

            Window!.AddFlags(WindowManagerFlags.KeepScreenOn);
        }

        public override bool OnKeyDown([GeneratedEnum] global::Android.Views.Keycode keyCode, KeyEvent? e)
        {
            if (_gameControllerService is not null && e is not null)
            {
                return _gameControllerService.OnKeyDown(keyCode, e) || base.OnKeyDown(keyCode, e);
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp([GeneratedEnum] global::Android.Views.Keycode keyCode, KeyEvent? e)
        {
            if (_gameControllerService is not null && e is not null)
            {
                return _gameControllerService.OnKeyUp(keyCode, e) || base.OnKeyUp(keyCode, e);
            }

            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnGenericMotionEvent(MotionEvent? e)
        {
            if (_gameControllerService is not null && e is not null)
            {
                return _gameControllerService.OnGenericMotionEvent(e) || base.OnGenericMotionEvent(e);
            }

            return base.OnGenericMotionEvent(e);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

