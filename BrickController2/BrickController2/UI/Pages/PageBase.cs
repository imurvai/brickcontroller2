using BrickController2.UI.ViewModels;
using Xamarin.Forms;

namespace BrickController2.UI.Pages
{
    public abstract class PageBase : ContentPage
    {
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        protected override void OnAppearing()
        {
            (BindingContext as PageViewModelBase)?.OnAppearing();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            (BindingContext as PageViewModelBase)?.OnDisappearing();
            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            var result = ((BindingContext as PageViewModelBase)?.OnBackButtonPressed()) ?? true;
            return result && base.OnBackButtonPressed();
        }
    }
}
