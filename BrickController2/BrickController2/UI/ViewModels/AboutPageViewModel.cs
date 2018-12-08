using BrickController2.PlatformServices.Versioning;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class AboutPageViewModel : PageViewModelBase
    {
        public AboutPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IVersionService versionService)
            : base(navigationService, translationService)
        {
            Version = versionService.ApplicationVersion;
        }

        public string Version { get; }
    }
}
