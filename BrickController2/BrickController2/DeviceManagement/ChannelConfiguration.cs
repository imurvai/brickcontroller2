using BrickController2.CreationManagement;

namespace BrickController2.DeviceManagement
{
    public struct ChannelConfiguration
    {
        public int Channel { get; set; }
        public ChannelOutputType ChannelOutputType { get; set; }
        public int MaxServoAngle { get; set; }
        public int ServoBaseAngle { get; set; }
    }
}
