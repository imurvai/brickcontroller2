using BrickController2.PlatformServices.Infrared;

namespace BrickController2.Windows.PlatformServices.Infrared;

public class InfraredService : IInfraredService
{
    public InfraredService()
    {
    }

    public bool IsInfraredSupported => false;

    public bool IsCarrierFrequencySupported(int carrierFrequency) => throw new NotImplementedException();

    public Task SendPacketAsync(int carrierFrequency, int[] packet) => throw new NotImplementedException();
}