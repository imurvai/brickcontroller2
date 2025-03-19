using BrickController2.PlatformServices.Infrared;
using System;
using System.Threading.Tasks;

namespace BrickController2.Windows.PlatformServices.Infrared;

public class InfraredService : IInfraredService
{
    public bool IsInfraredSupported => false;

    public bool IsCarrierFrequencySupported(int carrierFrequency) => throw new NotImplementedException();

    public Task SendPacketAsync(int carrierFrequency, int[] packet) => throw new NotImplementedException();
}