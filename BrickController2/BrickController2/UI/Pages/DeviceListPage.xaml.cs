using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
    public partial class DeviceListPage : PageBase
    {
        public DeviceListPage(ViewModelBase vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}