using BrickController2.Helpers;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public abstract class PageViewModelBase : NotifyPropertyChangedSource, IPageViewModel
    {
        protected PageViewModelBase(INavigationService navigationService, ITranslationService translationService)
        {
            NavigationService = navigationService;
            TranslationService = translationService;
        }

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
        public virtual bool OnBackButtonPressed() => true;

        protected INavigationService NavigationService { get; }
        protected ITranslationService TranslationService { get; }

        protected string Translate(string key) => TranslationService.Translate(key);
    }
}
