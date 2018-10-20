using System.Threading.Tasks;

namespace BrickController2.PlatformServices.Infrared
{
    public interface IInfraredService
    {
        bool IsInfraredSupported { get; }
        bool IsCarrierFrequencySupported(int carrierFrequency);

        Task SendPacketAsync(int carrierFrequency, int[] packet);
    }
}
