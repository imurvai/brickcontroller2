using System;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.MainThread
{
    public interface IMainThreadService
    {
        bool IsOnMainThread { get; }

        Task RunOnMainThread(Action action);
    }
}
