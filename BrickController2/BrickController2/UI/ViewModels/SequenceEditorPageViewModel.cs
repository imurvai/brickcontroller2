using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class SequenceEditorPageViewModel : PageViewModelBase
    {
        public SequenceEditorPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService) :
            base(navigationService, translationService)
        {
        }
    }
}
