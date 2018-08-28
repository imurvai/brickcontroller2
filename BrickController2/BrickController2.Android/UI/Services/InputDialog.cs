using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BrickController2.Droid.UI.Services
{
    public class InputDialog : Dialog
    {
        protected InputDialog(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public InputDialog(Context context)
            : base(context)
        {
        }

        protected InputDialog(Context context, bool cancelable, IDialogInterfaceOnCancelListener cancelListener)
            : base(context, cancelable, cancelListener)
        {
        }

        public InputDialog(Context context, int themeResId)
            : base(context, themeResId)
        {
        }

        protected InputDialog(Context context, bool cancelable, EventHandler cancelHandler)
            : base(context, cancelable, cancelHandler)
        {
        }
    }
}