using Android.App;
using Android.Content;
using Android.OS;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/SplashTheme.Splash", 
        MainLauncher = true, 
        NoHistory = true)]
    public class SplashActivity : MauiAppCompatActivity
    {
        public override void OnCreate(Bundle? savedInstanceState, PersistableBundle? persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(global::Android.App.Application.Context, typeof(MainActivity)));
        }

        public override void OnBackPressed()
        {
        }
    }
}