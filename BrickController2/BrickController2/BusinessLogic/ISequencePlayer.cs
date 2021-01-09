using BrickController2.CreationManagement;

namespace BrickController2.BusinessLogic
{
    public interface ISequencePlayer
    {
        void StartPlayer();
        void StopPlayer();

        void ToggleSequence(string deviceId, int channel, bool invert, Sequence sequence);
    }
}
