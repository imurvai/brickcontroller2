using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class AboutPage
	{
		public AboutPage(PageViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
		}
	}
}