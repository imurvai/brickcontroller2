using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.ViewModels;
using ZXing.Net.Maui;

namespace BrickController2.UI.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class BarcodeScannerPage
{
    public BarcodeScannerPage(PageViewModelBase vm, IBackgroundService backgroundService, IDialogServerHost dialogServerHost)
        : base(backgroundService, dialogServerHost)
    {
        InitializeComponent();
        AfterInitialize(vm);

        ViewModel = vm as BarcodeScannerPageViewModel;
    }

    public BarcodeScannerPageViewModel ViewModel { get; }

    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        ViewModel?.OnBarcodeDetected(e.Results);
    }
}