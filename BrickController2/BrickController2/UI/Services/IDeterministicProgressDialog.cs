using System;

namespace BrickController2.UI.Services
{
    public interface IDeterministicProgressDialog : IDisposable
    {
        void SetProgress(int value);
    }
}
