using System.Collections.Generic;
using BrickController2.UI.Navigation;
using Xamarin.Forms;

namespace BrickController2.UI.DI
{
    public delegate Page PageFactory(NavigationKey navigationKey, IDictionary<string, object> parameters);
}
