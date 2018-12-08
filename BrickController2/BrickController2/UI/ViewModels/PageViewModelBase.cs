using BrickController2.Helpers;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public abstract class PageViewModelBase : NotifyPropertyChangedSource
    {
        protected PageViewModelBase(INavigationService navigationService, ITranslationService translationService)
        {
            NavigationService = navigationService;
            TranslationService = translationService;
        }

        protected INavigationService NavigationService { get; }
        protected ITranslationService TranslationService { get; }

        public virtual void OnAppearing() { }
        public virtual void OnDisappearing() { }
        public virtual bool OnBackButtonPressed() => true;

        protected string Translate(string key) => TranslationService.Translate(key);
    }
}
