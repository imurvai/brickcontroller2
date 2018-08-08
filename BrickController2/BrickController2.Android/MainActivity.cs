using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Autofac;
using BrickController2.CreationManagement.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Droid.HardwareServices;
using BrickController2.Droid.HardwareServices.DI;
using BrickController2.HardwareServices;
using BrickController2.UI.DI;
using System;
using System.Collections.Generic;

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
            if (((int)e.Source & (int)InputSourceType.Gamepad) != 0 && e.RepeatCount == 0)
            {
                _gameControllerService.SendEvent(GameControllerEventType.Button, e.KeyCode.ToString(), 1.0F);
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (((int)e.Source & (int)InputSourceType.Gamepad) != 0 && e.RepeatCount == 0)
            {
                _gameControllerService.SendEvent(GameControllerEventType.Button, e.KeyCode.ToString(), 0.0F);
                return true;
            }

            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            if (e.Source == InputSourceType.Joystick && e.Action == MotionEventActions.Move)
            {
                var events = new Dictionary<(GameControllerEventType, string), float>();
                foreach (Axis axisCode in Enum.GetValues(typeof(Axis)))
                {
                    var axisValue = e.GetAxisValue(axisCode);
                    
                    if (Math.Abs(axisValue) < 0.1F)
                    {
                        axisValue = 0.0F;
                    }

                    events[(GameControllerEventType.Axis, axisCode.ToString())] = axisValue;
                }

                _gameControllerService.SendEvents(events);
                return true;
            }

            return base.OnGenericMotionEvent(e);
        }

        #endregion

        private IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new HardwareServicesModule());

            builder.RegisterModule(new CreationManagementModule());
            builder.RegisterModule(new DeviceManagementModule());
            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}

