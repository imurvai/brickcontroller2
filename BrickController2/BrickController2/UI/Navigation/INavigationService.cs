using BrickController2.UI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.UI.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync<T>(IDictionary<string, object> parameters = null) where T : ViewModelBase;

        Task NavigateBackAsync();
    }
}
