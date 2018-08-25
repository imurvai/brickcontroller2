using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerProfileDetailesPage
	{
		public ControllerProfileDetailesPage(PageViewModelBase vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}