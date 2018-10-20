using BrickController2.PlatformServices.Versioning;
using BrickController2.UI.Services.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class AboutPageViewModel : PageViewModelBase
    {
        public AboutPageViewModel(INavigationService navigationService, IVersionService versionService)
            : base(navigationService)
        {
            Version = versionService.ApplicationVersion;
        }

        public string Version { get; }
    }
}
