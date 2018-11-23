using System;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.UIThread
{
    public interface IUIThreadService
    {
        bool IsOnMainThread { get; }

        Task RunOnMainThread(Action action);
    }
}
