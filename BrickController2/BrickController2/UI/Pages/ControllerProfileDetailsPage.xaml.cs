using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerProfileDetailsPage
	{
		public ControllerProfileDetailsPage(PageViewModelBase vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}