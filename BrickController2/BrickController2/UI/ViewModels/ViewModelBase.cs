using System.ComponentModel;
using System.Runtime.CompilerServices;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.DI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected INavigationService NavigationService { get; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
