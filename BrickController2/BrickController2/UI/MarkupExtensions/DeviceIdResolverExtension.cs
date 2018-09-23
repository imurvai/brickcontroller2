using System;
using BrickController2.DeviceManagement;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.MarkupExtensions
{
    [ContentProperty(nameof(DeviceId))]
    public class DeviceIdResolverExtension : IMarkupExtension<string>
    {
        public string DeviceId { get; set; }

        public string ProvideValue(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private string ProvideValueInternal(IServiceProvider serviceProvider)
        {
            var deviceManager = serviceProvider.GetService<IDeviceManager>();

            if (deviceManager == null || string.IsNullOrEmpty(DeviceId))
            {
                return null;
            }

            return deviceManager.GetDeviceById(DeviceId)?.Name ?? "???";
        }
    }
}
