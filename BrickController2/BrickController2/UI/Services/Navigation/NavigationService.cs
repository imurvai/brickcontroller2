using System.Reflection;
using BrickController2.UI.DI;
using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly PageFactory _pageFactory;
        private readonly ViewModelFactory _viewModelFactory;

        public NavigationService(PageFactory pageFactory, ViewModelFactory viewModelFactory)
        {
            _pageFactory = pageFactory;
            _viewModelFactory = viewModelFactory;
        }

        public Task NavigateToAsync<T>(NavigationParameters parameters = null) where T : PageViewModelBase
        {
            var vm = _viewModelFactory(typeof(T), parameters);
            var page = _pageFactory(GetPageType<T>(), vm);
            return Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public Task NavigateToModalAsync<T>(NavigationParameters parameters = null) where T : PageViewModelBase
        {
            var vm = _viewModelFactory(typeof(T), parameters);
            var page = _pageFactory(GetPageType<T>(), vm);
            return Application.Current.MainPage.Navigation.PushModalAsync(page);
        }

        public Task NavigateBackAsync()
        {
            return Application.Current.MainPage.Navigation.PopAsync();
        }

        public Task NavigateModalBackAsync()
        {
            return Application.Current.MainPage.Navigation.PopAsync();
        }

        private Type GetPageType<T>() where T : PageViewModelBase
        {
            var pageTypeName = typeof(T).FullName?.Replace(".ViewModels.", ".Pages.").Replace("PageViewModel", "Page");
            var pageType = pageTypeName != null ? Assembly.GetExecutingAssembly().GetType(pageTypeName) : null;
            return pageType;
        }
    }
}
