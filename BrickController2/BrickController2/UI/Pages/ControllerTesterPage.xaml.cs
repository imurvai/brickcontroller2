using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerTesterPage
	{
		public ControllerTesterPage(PageViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
		}
	}
}