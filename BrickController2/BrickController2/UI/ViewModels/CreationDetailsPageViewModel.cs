using System.Collections.Generic;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class CreationDetailsPageViewModel : PageViewModelBase
    {
        public CreationDetailsPageViewModel(INavigationService navigationService, IDictionary<string, object> parameters)
            : base(navigationService)
        {
        }
    }
}
