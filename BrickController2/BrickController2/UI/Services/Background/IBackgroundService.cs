using System;

namespace BrickController2.UI.Services.Background
{
    public interface IBackgroundService
    {
        event EventHandler<EventArgs> ApplicationSleepEvent;
        event EventHandler<EventArgs> ApplicationResumeEvent;
    }
}
