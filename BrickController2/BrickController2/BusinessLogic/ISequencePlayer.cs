using BrickController2.CreationManagement;
using System.Threading.Tasks;

namespace BrickController2.BusinessLogic
{
    public interface ISequencePlayer
    {
        Task StartPlayerAsync();
        Task StopPlayerAsync();

        void ToggleSequence(string deviceId, int channel, Sequence sequence);
    }
}
