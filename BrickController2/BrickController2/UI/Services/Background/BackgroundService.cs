using System;

namespace BrickController2.UI.Services.Background
{
    public class BackgroundService : IBackgroundService
    {
        public event EventHandler<EventArgs>? ApplicationSleepEvent;
        public event EventHandler<EventArgs>? ApplicationResumeEvent;

        internal void FireApplicationSleepEvent()
        {
            ApplicationSleepEvent?.Invoke(this, EventArgs.Empty);
        }

        internal void FireApplicationResumeEvent()
        {
            ApplicationResumeEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
