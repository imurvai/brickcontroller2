using Android.Content;
using Android.Hardware;
using BrickController2.HardwareServices;
using System;
using System.Threading.Tasks;

namespace BrickController2.Droid.HardwareServices
{
    public class InfraredService : IInfraredService
    {
        private readonly ConsumerIrManager _irManager;

        public InfraredService(Context context)
        {
            var irManager = (ConsumerIrManager)context.GetSystemService(Context.ConsumerIrService);
            if (irManager != null && irManager.HasIrEmitter)
            {
                _irManager = irManager;
            }
        }

        public bool IsInfraredSupported => _irManager != null;

        public bool IsCarrierFrequencySupported(int carrierFrequency)
        {
            if (_irManager == null)
            {
                return false;
            }

            var frequencyRanges = _irManager.GetCarrierFrequencies();
            foreach (var range in frequencyRanges)
            {
                if (range.MinFrequency <= carrierFrequency && carrierFrequency <= range.MaxFrequency)
                {
                    return true;
                }
            }

            return false;
        }

        public Task SendPacketAsync(int carrierFrequency, int[] packet)
        {
            if (_irManager == null)
            {
                throw new InvalidOperationException("Infrared is not supported.");
            }

            return _irManager.TransmitAsync(carrierFrequency, packet);
        }
    }
}