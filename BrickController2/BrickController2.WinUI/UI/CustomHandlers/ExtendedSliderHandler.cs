using BrickController2.UI.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;

namespace BrickController2.Windows.UI.CustomHandlers;

public class ExtendedSliderHandler : SliderHandler
{
    public static readonly PropertyMapper<ExtendedSlider, ExtendedSliderHandler> PropertyMapper = new(Mapper)
    {
        [ExtendedSlider.StepProperty.PropertyName] = ApplyStep
    };

    public ExtendedSliderHandler() : base(PropertyMapper)
    {
    }
    private ExtendedSlider Slider => VirtualView as ExtendedSlider;

    protected override void ConnectHandler(Microsoft.UI.Xaml.Controls.Slider platformView)
    {
        base.ConnectHandler(platformView);

        platformView.Loaded += OnPlatformViewLoaded;
        platformView.PointerCaptureLost += OnPlatformView_PointerCaptureLost;
    }

    protected override void DisconnectHandler(Microsoft.UI.Xaml.Controls.Slider platformView)
    {
        platformView.Loaded -= OnPlatformViewLoaded;
        platformView.PointerCaptureLost -= OnPlatformView_PointerCaptureLost;

        base.DisconnectHandler(platformView);
    }

    private void OnPlatformView_PointerCaptureLost(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        => Slider?.TouchUp();

    void OnPlatformViewLoaded(object sender, RoutedEventArgs e)
    {
        ApplyStep(this, Slider);
    }

    private static void ApplyStep(ExtendedSliderHandler handler, ExtendedSlider slider)
    {
        handler.PlatformView.StepFrequency = slider.Step;
        handler.PlatformView.SmallChange = slider.Step;
    }
}