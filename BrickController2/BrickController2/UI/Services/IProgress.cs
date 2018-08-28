using System;

namespace BrickController2.UI.Services
{
    public interface IProgress : IDisposable
    {
        int Progress { get; set; }
    }
}
