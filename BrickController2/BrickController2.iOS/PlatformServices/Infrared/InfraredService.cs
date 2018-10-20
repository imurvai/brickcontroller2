using BrickController2.PlatformServices.Infrared;
using System;
using System.Threading.Tasks;

namespace BrickController2.iOS.PlatformServices.Infrared
{
    public class InfraredService : IInfraredService
    {
        public bool IsInfraredSupported => false;

        public bool IsCarrierFrequencySupported(int carrierFrequency)
        {
            return false;
        }

        public Task SendPacketAsync(int carrierFrequency, int[] packet)
        {
            throw new NotImplementedException();
        }
    }
}