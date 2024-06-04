using BrickController2.UI.ViewModels;
using System.Threading.Tasks;

namespace BrickController2.UI.Services.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase;
        Task NavigateToModalAsync<T>(NavigationParameters? parameters = null) where T : PageViewModelBase;
        Task NavigateBackAsync();
        Task NavigateModalBackAsync();
    }
}
