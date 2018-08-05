using System.Collections.Generic;
using System.Windows.Input;
using BrickController2.UI.Navigation;
using Xamarin.Forms;

namespace BrickController2.UI.DI
{
    public class CreationListViewModel : ViewModelBase
    {
        public CreationListViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            NavigateToCreationDetailsCommand =
                new Command(async () =>
                {
                    var parameters = new Dictionary<string, object>();
                    parameters["key1"] = "value1";
                    parameters["key2"] = "value2";
                    await NavigationService.NavigateToAsync(NavigationKey.CreationDetails, parameters);
                });
        }

        public ICommand NavigateToCreationDetailsCommand { get; }
    }
}