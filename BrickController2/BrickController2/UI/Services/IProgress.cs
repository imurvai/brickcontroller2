using System;

namespace BrickController2.UI.Services
{
    public interface IProgress : IDisposable
    {
        string Title { set; }
        string Message { set; }
        int Percent { get; set; }
    }
}
