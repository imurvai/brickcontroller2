using System.Collections.Generic;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class CreationDetailsViewModel : ViewModelBase
    {
        public CreationDetailsViewModel(INavigationService navigationService, IDictionary<string, object> parameters)
            : base(navigationService)
        {
        }
    }
}
