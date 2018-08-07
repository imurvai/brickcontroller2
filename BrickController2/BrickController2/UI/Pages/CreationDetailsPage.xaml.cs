using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class CreationDetailsPage
	{
		public CreationDetailsPage(ViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
		}
	}
}