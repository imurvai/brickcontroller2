using Android.Widget;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using BrickController2.UI.Controls;

namespace BrickController2.Android.UI.CustomHandlers
{
    internal class ExtendedSliderHandler : SliderHandler
    {
        public static readonly PropertyMapper<ExtendedSlider, ExtendedSliderHandler> _PropertyMapper = new(SliderHandler.Mapper)
        {
            //[nameof(BrickController2.UI.Controls.ExtendedSlider.Value)] = (handler, slider) => handler.PlatformView?.Step = slider.Step
        };

        private ExtendedSlider? ExtendedSlider => (VirtualView as ExtendedSlider);

        public ExtendedSliderHandler() : base(_PropertyMapper)
        {
        }

        protected override void ConnectHandler(SeekBar platformView)
        {
            base.ConnectHandler(platformView);

            platformView.StartTrackingTouch += StartTrackingTouch;
            platformView.StopTrackingTouch += StopTrackingTouch;
            platformView.ProgressChanged += ProgressChanged;
        }

        protected override void DisconnectHandler(SeekBar platformView)
        {
            platformView.StartTrackingTouch -= StartTrackingTouch;
            platformView.StopTrackingTouch -= StopTrackingTouch;
            platformView.ProgressChanged -= ProgressChanged;

            platformView.Dispose();
            base.DisconnectHandler(platformView);
        }

        private void StartTrackingTouch(object? sender, SeekBar.StartTrackingTouchEventArgs e)
        {
            ExtendedSlider?.TouchDown();
        }

        private void StopTrackingTouch(object? sender, SeekBar.StopTrackingTouchEventArgs e)
        {
            ExtendedSlider?.TouchUp();
        }

        private void ProgressChanged(object? sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (ExtendedSlider is null || !e.FromUser)
            {
                return;
            }

            ExtendedSlider.Value = ExtendedSlider.Minimum + ((ExtendedSlider.Maximum - ExtendedSlider.Minimum) * e.Progress / SliderExtensions.PlatformMaxValue);
        }
    }
}
