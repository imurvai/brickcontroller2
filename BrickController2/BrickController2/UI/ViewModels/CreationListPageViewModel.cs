using System.Collections.ObjectModel;
using System.Windows.Input;
using BrickController2.UI.Navigation;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        public CreationListPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            NavigateToCreationDetailsCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<CreationDetailsPageViewModel>(new NavigationParameters(("key1", "value1"), ("key2", "value2")));
            });

            NavigeteToDeviceListCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<DeviceListPageViewModel>(null);
            });

            NavigateToControllerTesterCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<ControllerTesterPageViewModel>();
            });
        }

        public ICommand NavigateToCreationDetailsCommand { get; }
        public ICommand NavigeteToDeviceListCommand { get; }
        public ICommand NavigateToControllerTesterCommand { get; }

        public ObservableCollection<string> Creations { get; } = new ObservableCollection<string>();
    }
}