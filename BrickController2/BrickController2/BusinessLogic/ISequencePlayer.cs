using BrickController2.CreationManagement;
using System.Threading.Tasks;

namespace BrickController2.BusinessLogic
{
    public interface ISequencePlayer
    {
        Task ToggleSequenceAsync(string deviceId, int channel, Sequence sequence);
        Task StopAllSequencesAsync();
    }
}
