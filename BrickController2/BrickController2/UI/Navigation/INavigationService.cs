using BrickController2.UI.ViewModels;
using System.Threading.Tasks;

namespace BrickController2.UI.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync<T>(NavigationParameters parameters = null) where T : ViewModelBase;
        Task NavigateToModalAsync<T>(NavigationParameters parameters = null) where T : ViewModelBase;
        Task NavigateBackAsync();
        Task NavigateModalBackAsync();
    }
}
