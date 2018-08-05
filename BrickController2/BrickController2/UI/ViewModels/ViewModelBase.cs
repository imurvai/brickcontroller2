using System.Collections.Generic;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.ViewModels
{
    public abstract class ViewModelBase
    {
        protected ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public IDictionary<string, object> InitParameters { get; set; }
        protected INavigationService NavigationService { get; }
    }
}
