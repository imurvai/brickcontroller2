using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerActionPage
	{
		public ControllerActionPage(PageViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
        }
    }
}