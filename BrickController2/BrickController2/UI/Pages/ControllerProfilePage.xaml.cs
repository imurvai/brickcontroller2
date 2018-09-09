using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerProfilePage
	{
		public ControllerProfilePage(PageViewModelBase vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}