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

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
        public virtual bool OnBackButtonPressed() { return true; }
    }
}
