using System;
using System.Threading.Tasks;
using BrickController2.Helpers;
using BrickController2.UI.Services.Navigation;

namespace BrickController2.UI.ViewModels
{
    public abstract class PageViewModelBase : NotifyPropertyChangedSource
    {
        protected PageViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        protected INavigationService NavigationService { get; }

        #region View lifecycle callbacks

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
        public virtual bool OnBackButtonPressed() { return true; }

        #endregion

        #region DisplayAlert

        public Func<string, string, string, string, Task> OnDisplayAlert { get; set; }

        protected Task DisplayAlertAsync(string title, string message, string cancel)
        {
            return OnDisplayAlert?.Invoke(title, message, null, cancel);
        }

        protected Task DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            return OnDisplayAlert?.Invoke(title, message, accept, cancel);
        }

        #endregion

        #region DisplayActionSheet

        public Func<string, string, string, string[], Task<string>> OnDisplayActionSheet { get; set; }

        protected Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
        {
            return OnDisplayActionSheet?.Invoke(title, cancel, destruction, buttons);
        }

        #endregion
    }
}
