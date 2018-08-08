using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrickController2.UI.DI;
using BrickController2.UI.Pages;
using BrickController2.UI.ViewModels;
using Xamarin.Forms;

namespace BrickController2.UI.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly PageFactory _pageFactory;
        private readonly ViewModelFactory _viewModelFactory;
        private readonly IDictionary<Type, Type> _pageViewModelMap = new Dictionary<Type, Type>
        {
            { typeof(CreationListViewModel), typeof(CreationListPage) },
            { typeof(CreationDetailsViewModel), typeof(CreationDetailsPage) },
            { typeof(DeviceListViewModel), typeof(DeviceListPage) },
            { typeof(ControllerTesterViewModel), typeof(ControllerTesterPage) }
        };

        public NavigationService(PageFactory pageFactory, ViewModelFactory viewModelFactory)
        {
            _pageFactory = pageFactory;
            _viewModelFactory = viewModelFactory;
        }

        public Task NavigateToAsync<T>(NavigationParameters parameters = null) where T : ViewModelBase
        {
            var vm = _viewModelFactory(typeof(T), parameters);
            var page = _pageFactory(GetPageType<T>(), vm);
            return Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public Task NavigateToModalAsync<T>(NavigationParameters parameters = null) where T : ViewModelBase
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

        private Type GetPageType<T>() where T : ViewModelBase
        {
            if (!_pageViewModelMap.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T).Name} is not registered for navigation.");
            }

            return _pageViewModelMap[typeof(T)];
        }
    }
}
