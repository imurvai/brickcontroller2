using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;

namespace BrickController2.BusinessLogic
{
    public interface ISequencePlayer
    {
        void ToggleSequence(Device device, int channel, Sequence sequence);
        void StopAllSequences();
    }
}
