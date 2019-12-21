using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class SequenceListPageViewModel : PageViewModelBase
    {
        public SequenceListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService) :
            base(navigationService, translationService)
        {
        }
    }
}
