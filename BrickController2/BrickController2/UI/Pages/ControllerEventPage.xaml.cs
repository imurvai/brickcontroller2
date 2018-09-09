using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerEventPage
	{
		public ControllerEventPage(PageViewModelBase vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
	}
}