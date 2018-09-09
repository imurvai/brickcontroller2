using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class PlayerPage
	{
		public PlayerPage(PageViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
		}
	}
}