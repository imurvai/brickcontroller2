using BrickController2.UI.Pages;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;

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
            var window = page.Window;

            window.SizeChanged += Window_SizeChanged;
            page.Unloaded += (sender, args) => window.SizeChanged -= Window_SizeChanged;

            ApplyTitleViewWidth(page);
        }
    }

    private void Window_SizeChanged(object sender, EventArgs e)
    {
        ApplyTitleViewWidth(VirtualView as PageBase);
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
