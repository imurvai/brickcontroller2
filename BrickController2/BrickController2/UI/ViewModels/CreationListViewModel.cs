using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BrickController2.UI.Navigation;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class CreationListViewModel : ViewModelBase
    {
        public CreationListViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            NavigateToCreationDetailsCommand = new Command(async () =>
            {
                var parameters = new Dictionary<string, object>();
                parameters["key1"] = "value1";
                parameters["key2"] = "value2";
                await NavigationService.NavigateToAsync<CreationDetailsViewModel>(parameters);
            });

            NavigeteToDeviceListCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<DeviceListViewModel>(null);
            });
        }

        public ICommand NavigateToCreationDetailsCommand { get; }
        public ICommand NavigeteToDeviceListCommand { get; }

        public ObservableCollection<string> Creations { get; } = new ObservableCollection<string>();
    }
}