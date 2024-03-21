using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BrickController2.Helpers
{
    public abstract class NotifyPropertyChangedSource : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged is null)
                return;

            MainThread.BeginInvokeOnMainThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
