using BrickController2.UI.ViewModels;
using Xamarin.Forms;

namespace BrickController2.UI.Pages
{
    public class PageBase : ContentPage
    {
        protected override void OnAppearing()
        {
            var vm = BindingContext as ViewModelBase;
            vm?.OnAppearing();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            var vm = BindingContext as ViewModelBase;
            vm?.OnDisappearing();

            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            var vm = BindingContext as ViewModelBase;
            vm?.OnBackButtonPressed();

            return base.OnBackButtonPressed();
        }
    }
}
