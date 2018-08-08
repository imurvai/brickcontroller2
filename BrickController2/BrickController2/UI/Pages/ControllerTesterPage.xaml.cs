using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerTesterPage
	{
		public ControllerTesterPage(ViewModelBase vm)
		{
			InitializeComponent();
            BindingContext = vm;
		}
	}
}