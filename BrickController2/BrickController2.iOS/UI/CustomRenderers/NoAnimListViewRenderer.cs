using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using BrickController2.iOS.UI.CustomRenderers;

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