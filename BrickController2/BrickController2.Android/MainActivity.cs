using Android.App;
using Android.Content.PM;
using Android.OS;
using Autofac;
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
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var container = InitDI();
            var app = container.Resolve<App>();
            LoadApplication(app);
        }

        private IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}

