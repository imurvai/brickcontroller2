using BrickController2.iOS.UI.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ListView), typeof(NoAnimListViewRenderer))]
namespace BrickController2.iOS.UI.CustomRenderers
{
    public class NoAnimListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            AnimationsEnabled = false;
        }
    }
}