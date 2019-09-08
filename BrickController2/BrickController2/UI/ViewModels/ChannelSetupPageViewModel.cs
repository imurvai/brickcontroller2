using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class ChannelSetupPageViewModel : PageViewModelBase
    {
        public ChannelSetupPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService)
            : base(navigationService, translationService)
        {
        }
    }
}
