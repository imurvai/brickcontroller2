using BrickController2.UI.Services.Dialog;

namespace BrickController2.UI.ViewModels
{
    public interface IPageViewModel
    {
        void OnAppearing();
        void OnDisappearing();
        bool OnBackButtonPressed();
    }
}
