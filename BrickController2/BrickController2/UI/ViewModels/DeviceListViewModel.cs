using BrickController2.UI.Navigation;

namespace BrickController2.UI.DI
{
    public class DeviceListViewModel : ViewModelBase
    {
        public DeviceListViewModel(INavigationService navigationService) 
            : base(navigationService)
        {
        }
    }
}
