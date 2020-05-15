using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
	public partial class ControllerProfilePage
	{
		public ControllerProfilePage(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost)
            : base(backgroundService, dialogServerHost)
        {
            InitializeComponent();
            AfterInitialize(vm);
        }
    }
}