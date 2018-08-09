using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
    public partial class DeviceListPage
    {
        public DeviceListPage(PageViewModelBase vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}