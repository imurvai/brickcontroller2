using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.UI.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync(NavigationKey navigationKey, IDictionary<string, object> parameters = null);

        Task NavigateBackAsync();
    }
}
