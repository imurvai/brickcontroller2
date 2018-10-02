using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;

namespace BrickController2.UI.ViewModels
{
    public class AboutPageViewModel : PageViewModelBase
    {
        public AboutPageViewModel(INavigationService navigationService, IDialogService dialogService)
            : base(navigationService)
        {
        }

        public string Version { get; } = "1.0";
    }
}
