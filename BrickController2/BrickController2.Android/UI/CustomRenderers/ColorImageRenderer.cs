//using System.ComponentModel;
//using Android.Content;
//using Android.Graphics;
//using Microsoft.Maui.Controls.Compatibility;
//using Microsoft.Maui.Controls.Compatibility.Platform.Android;
//using Microsoft.Maui.Controls.Platform;
//using BrickController2.Droid.UI.CustomRenderers;
//using BrickController2.UI.Controls;

//[assembly:ExportRenderer(typeof(ColorImage), typeof(ColorImageRenderer))]
//namespace BrickController2.Droid.UI.CustomRenderers
//{
//#pragma warning disable CS0618 // Type or member is obsolete
//    public class ColorImageRenderer : ImageRenderer
//#pragma warning restore CS0618 // Type or member is obsolete
//    {
//        public ColorImageRenderer(Context context) : base(context)
//        {
//        }

//        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
//        {
//            base.OnElementChanged(e);
//            SetColor();
//        }

//        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            base.OnElementPropertyChanged(sender, e);

//            if (e.PropertyName == ColorImage.ColorProperty.PropertyName || e.PropertyName == ColorImage.SourceProperty.PropertyName)
//            {
//                SetColor();
//            }
//        }

//        private void SetColor()
//        {
//            if (Control is null || !(Element is ColorImage colorImage))
//            {
//                return;
//            }

//            if (colorImage.Color.Equals(Colors.Transparent))
//            {
//                if (Control.ColorFilter is not null)
//                {
//                    Control.ClearColorFilter();
//                }
//            }
//            else
//            {
//                var colorFilter = new PorterDuffColorFilter(colorImage.Color.ToAndroid(), PorterDuff.Mode.SrcIn!);
//                Control.SetColorFilter(colorFilter);
//            }
//        }
//    }
//}