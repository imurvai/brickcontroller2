using BrickController2.UI.Pages;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace BrickController2.Windows.UI.CustomHandlers;

/// <summary>
/// Applies TitleView adjustments to resolve issue: TitleView content does not expand across the entire width
/// https://github.com/dotnet/maui/issues/10703
/// </summary>
internal class CustomPageHandler : PageHandler
{
    protected override void ConnectHandler(ContentPanel platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is PageBase page)
        {
            page.Window.SizeChanged += (sender, args) => ApplyTitleViewWidth(page);
            ApplyTitleViewWidth(page);
        }
    }

    private static void ApplyTitleViewWidth(PageBase page)
    {
        var view = NavigationPage.GetTitleView(page);
        if (view != null)
        {
            view.WidthRequest = page.Window.Width;
        }
    }
}
