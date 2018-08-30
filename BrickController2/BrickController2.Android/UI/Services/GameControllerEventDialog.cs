using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using BrickController2.Droid.HardwareServices;

namespace BrickController2.Droid.UI.Services
{
    public class GameControllerEventDialog : AlertDialog
    {
        private readonly GameControllerService _gameControllerService;

        public GameControllerEventDialog(Context context, GameControllerService gameControllerService) : this(context)
        {
            _gameControllerService = gameControllerService;
        }

        protected GameControllerEventDialog(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        protected GameControllerEventDialog(Context context) : base(context) { }
        protected GameControllerEventDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener) : base(context, cancelable, cancelListener) { }
        protected GameControllerEventDialog(Context context, int themeResId) : base(context, themeResId) { }
        protected GameControllerEventDialog(Context context, bool cancelable, EventHandler cancelHandler) : base(context, cancelable, cancelHandler) { }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            return _gameControllerService.OnKeyDown(keyCode, e) || base.OnKeyDown(keyCode, e);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            return _gameControllerService.OnKeyUp(keyCode, e) || base.OnKeyUp(keyCode, e);
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            return _gameControllerService.OnGenericMotionEvent(e) || base.OnGenericMotionEvent(e);
        }
    }
}