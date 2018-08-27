using System;

namespace BrickController2.UI.Services
{
    public interface IInputDialogResult
    {
        bool IsPositive { get; }
        string Result { get; }
    }
}
