using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Translation;
using System.Linq;

namespace BrickController2.Helpers
{
    public static class DeviceExtensions
    {
        public static string GetChannelName(this Device device, int channel, ITranslationService translationService = null)
        {
            var channelName = device?.RegisteredPorts.FirstOrDefault(p => p.Channel == channel)?.Name;

            if (!string.IsNullOrEmpty(channelName))
            {
                channelName = translationService?.Translate(channelName);
            }

            return channelName ?? $"{channel + 1}";
        }
    }
}
