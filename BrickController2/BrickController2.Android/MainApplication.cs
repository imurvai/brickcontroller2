using Android.App;
using Android.Runtime;
using Microsoft.Maui.Controls.Compatibility.Hosting;

namespace BrickController2.Droid
{
#if DEBUG
[Application(Debuggable = true)]
#else
[Application(Debuggable = false)]
#endif
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
                : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiCompatibility();
            return builder.Build();
        }
    }
}