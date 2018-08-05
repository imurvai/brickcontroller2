using System.Collections.Generic;
using BrickController2.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BrickController2.UI.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CreationDetailsPage : ContentPage
	{
		public CreationDetailsPage(ViewModelFactory viewModelFactory, IDictionary<string, object> parameters)
		{
			InitializeComponent();
		    BindingContext = viewModelFactory(typeof(CreationDetailsViewModel), parameters);
		}
	}
}