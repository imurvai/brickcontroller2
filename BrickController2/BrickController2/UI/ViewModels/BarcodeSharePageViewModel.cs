using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels;

public class BarcodeSharePageViewModel : PageViewModelBase
{
    private readonly ICreationManager _creationManager;

    private string _barcodeValue;

    public BarcodeSharePageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        ICreationManager creationManager,
        NavigationParameters parameters)
        : base(navigationService, translationService)
    {
        _creationManager = creationManager;

        Item = parameters.Get<Creation>("item");

        ExportCommand = new SafeCommand(ExportAsync);
    }

    public Creation Item { get; }

    public BarcodeFormat BarcodeFormat { get; } = BarcodeFormat.QrCode;

    public string BarcodeValue
    {
        get { return _barcodeValue; }
        set
        {
            _barcodeValue = value;
            RaisePropertyChanged();
        }
    }

    public ICommand ExportCommand { get; }

    public override async void OnAppearing()
    {
        // build JSON payload
        BarcodeValue = await _creationManager.ExportCreationAsync(Item);
    }

    private async Task ExportAsync()
    { 
        //TODO
    }
}
