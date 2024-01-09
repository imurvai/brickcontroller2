using BrickController2.CreationManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZXing.Net.Maui;

namespace BrickController2.UI.ViewModels
{
    public class BarcodeScannerPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;
        private bool _scanningEnabled;
        private CancellationTokenSource _disappearingTokenSource;

        public BarcodeScannerPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;

            ImportCommand = new SafeCommand(ImportAsync, () => ScanningEnabled && Barcodes.Any());
        }

        public ObservableCollection<string> Barcodes { get; } = [];

        public bool ScanningEnabled
        {
            get { return _scanningEnabled; }
            set
            {
                _scanningEnabled = value;
                RaisePropertyChanged();
            }
        }

        public BarcodeReaderOptions ReaderOptions { get; } = new BarcodeReaderOptions
        {
            AutoRotate = true,
            Formats = BarcodeFormat.QrCode,
            Multiple = false
        };

        public ICommand ImportCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();

            // enable scanning
            ScanningEnabled = true;
        }

        public override void OnDisappearing()
        {
            // disable scanning
            ScanningEnabled = false;

            _disappearingTokenSource.Cancel();
        }

        internal void OnBarcodeDetected(BarcodeResult[] results)
        {
            foreach (BarcodeResult result in results)
            {
                if (!Barcodes.Contains(result.Value))
                {
                    Barcodes.Add(result.Value);
                }
            }
        }

        private async Task ImportAsync()
        {
            // disable scanning
            ScanningEnabled = false;

            int errorCounter = 0;
            foreach (var barcodeValue in Barcodes)
            {
                try
                {
                    await _creationManager.ImportCreationPayloadAsync(barcodeValue);
                }
                catch
                {
                    ++errorCounter;
                }
            }

            await _dialogService.ShowMessageBoxAsync("Imported", $"Imported {Barcodes.Count - errorCounter} of {Barcodes.Count}.", "OK", _disappearingTokenSource.Token);

            // clear imported codes and enable scanning
            Barcodes.Clear();
            ScanningEnabled = true;
        }
    }
}
