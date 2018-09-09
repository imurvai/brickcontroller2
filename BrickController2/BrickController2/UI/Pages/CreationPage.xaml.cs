using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class CreationPage
	{
		public CreationPage(PageViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
		}
	}
}