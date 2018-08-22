using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class DeviceDetailsPage
	{
		public DeviceDetailsPage(PageViewModelBase vm)
		{
		    InitializeComponent();
		    BindingContext = vm;
		}
    }
}