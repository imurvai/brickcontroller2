using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class ControllerProfileDetailsPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;

        public ControllerProfileDetailsPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;

            ControllerProfile = parameters.Get<ControllerProfile>("controllerprofile");
        }

        public ControllerProfile ControllerProfile { get; }
    }
}
