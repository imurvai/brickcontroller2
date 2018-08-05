using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrickController2.UI.Pages;
using Xamarin.Forms;

namespace BrickController2.UI.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly PageFactory _pageFactory;

        public NavigationService(PageFactory pageFactory)
        {
            _pageFactory = pageFactory;
        }

        public Task NavigateToAsync(NavigationKey navigationKey, IDictionary<string, object> parameters = null)
        {
            var page = _pageFactory(navigationKey, parameters);
            return Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public Task NavigateBackAsync()
        {
            return Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
