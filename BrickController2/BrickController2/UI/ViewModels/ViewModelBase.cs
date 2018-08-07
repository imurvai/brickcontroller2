using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected INavigationService NavigationService { get; }

        #region View lifecycle callbacks

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
        public virtual void OnBackButtonPressed() { }

        #endregion

        #region DisplayAlert

        public Func<string, string, string, string, Task> OnDisplayAlert { get; set; }

        protected Task DisplayAlert(string title, string message, string cancel)
        {
            return OnDisplayAlert?.Invoke(title, message, null, cancel);
        }

        protected Task DisplayAlert(string title, string message, string accept, string cancel)
        {
            return OnDisplayAlert?.Invoke(title, message, accept, cancel);
        }

        #endregion

        #region DisplayActionSheet

        public Func<string, string, string, string[], Task<string>> OnDisplayActionSheet { get; set; }

        protected Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons)
        {
            return OnDisplayActionSheet?.Invoke(title, cancel, destruction, buttons);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
