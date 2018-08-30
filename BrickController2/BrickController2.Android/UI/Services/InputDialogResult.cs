using BrickController2.UI.Services;

namespace BrickController2.Droid.UI.Services
{
    public class InputDialogResult : IInputDialogResult
    {
        public bool IsPositive { get; set; }
        public string Result { get; set; }
    }
}