using BrickController2.UI.ViewModels;
using Xamarin.Forms;

namespace BrickController2.UI.Pages
{
    public abstract class PageBase : ContentPage
    {
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            SetViewModelDisplayCallbacks();
        }

        protected override void OnAppearing()
        {
            (BindingContext as PageViewModelBase)?.OnAppearing();
            base.OnAppearing();
            SetViewModelDisplayCallbacks();
        }

        protected override void OnDisappearing()
        {
            (BindingContext as PageViewModelBase)?.OnDisappearing();
            base.OnDisappearing();
            ClearViewModelDisplayCallbacks();
        }

        protected override bool OnBackButtonPressed()
        {
            (BindingContext as PageViewModelBase)?.OnBackButtonPressed();
            return base.OnBackButtonPressed();
        }

        private void SetViewModelDisplayCallbacks()
        {
            if (BindingContext is PageViewModelBase vm)
            {
                vm.OnDisplayAlert = (title, message, accept, cancel) => string.IsNullOrEmpty(accept) ?
                    DisplayAlert(title, message, cancel) :
                    DisplayAlert(title, message, accept, cancel);

                vm.OnDisplayActionSheet = (title, cancel, destruction, buttons) => DisplayActionSheet(title, cancel, destruction, buttons);
            }
        }

        private void ClearViewModelDisplayCallbacks()
        {
            if (BindingContext is PageViewModelBase vm)
            {
                vm.OnDisplayAlert = null;
                vm.OnDisplayActionSheet = null;
            }
        }
    }
}
