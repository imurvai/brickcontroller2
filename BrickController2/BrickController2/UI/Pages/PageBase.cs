using BrickController2.UI.ViewModels;
using Xamarin.Forms;

namespace BrickController2.UI.Pages
{
    public abstract class PageBase : ContentPage
    {
        protected override void OnAppearing()
        {
            (BindingContext as ViewModelBase)?.OnAppearing();
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            (BindingContext as ViewModelBase)?.OnDisappearing();
            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as ViewModelBase)?.OnBackButtonPressed();
            return base.OnBackButtonPressed();
        }
    }
}
