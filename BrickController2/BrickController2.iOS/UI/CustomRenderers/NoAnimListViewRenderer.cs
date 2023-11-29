using Microsoft.Maui.Controls.Platform;

namespace BrickController2.iOS.UI.CustomRenderers
{
    public class NoAnimListViewRenderer : Microsoft.Maui.Controls.Handlers.Compatibility.ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            AnimationsEnabled = false;
        }
    }
}