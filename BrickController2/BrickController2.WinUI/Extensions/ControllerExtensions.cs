using Windows.Gaming.Input;

namespace BrickController2.Windows.Extensions;

public static class ControllerExtensions
{
    public static string GetDeviceId(this Gamepad gamepad) =>
        // kinda hack
        gamepad.User.NonRoamableId;
}
