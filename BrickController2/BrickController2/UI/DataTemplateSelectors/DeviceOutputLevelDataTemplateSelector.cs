using BrickController2.DeviceManagement;
using System;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.DataTemplateSelectors
{
    public class DeviceOutputLevelDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EmptyOutputLevelSelectorTemplate { get; set; }
        public DataTemplate BuWizzOutputLevelSelectorTemplate { get; set; }
        public DataTemplate BuWizz2OutputLevelSelectorTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Device device = item as Device;
            if (device == null)
            {
                throw new ArgumentException();
            }

            switch (device.DeviceType)
            {
                case DeviceType.BuWizz:
                    return BuWizzOutputLevelSelectorTemplate;

                case DeviceType.BuWizz2:
                    return BuWizz2OutputLevelSelectorTemplate;

                default:
                    return EmptyOutputLevelSelectorTemplate;
            }
        }
    }
}
