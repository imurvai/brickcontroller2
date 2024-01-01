using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BrickController2.Windows.UI.CustomHandlers;

public class CustomSwipeViewHandler : SwipeViewHandler
{
    protected override void ConnectHandler(SwipeControl platformView)
    {
        base.ConnectHandler(platformView);

        platformView.RightTapped += SwipeControl_RightTapped;
    }

    private void SwipeControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        // invoke command of the first left item to suppport deletion (workaround for Windouws without touch controls)
        if (VirtualView.LeftItems.Count == 1)
        {
            var item = VirtualView.LeftItems.First();
            item.OnInvoked();
        }
    }

    protected override void DisconnectHandler(SwipeControl platformView)
    {
        platformView.RightTapped -= SwipeControl_RightTapped;

        base.DisconnectHandler(platformView);
    }
}