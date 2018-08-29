using System;

namespace BrickController2.UI.Services
{
    public interface IProgress : IDisposable
    {
        string Title { get; set; }
        string Message { get; set; }
        int Progress { get; set; }
    }
}
