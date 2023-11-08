using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.AppCompat.App;

namespace BrickController2.Droid
{
    [Activity(
        Label = "BrickController2",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/SplashTheme.Splash", 
        MainLauncher = true, 
        NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(Android.App.Application.Context, typeof(MainActivity)));
        }

        public override void OnBackPressed()
        {
        }
    }
}