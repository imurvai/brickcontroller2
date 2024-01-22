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
        private string _currentValue;
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

            ImportCommand = new SafeCommand(ImportAsync, () => ScanningEnabled && CurrentValue is not null);
        }

        public bool ScanningEnabled
        {
            get { return _scanningEnabled; }
            set
            {
                _scanningEnabled = value;
                RaisePropertyChanged();
            }
        }

        public string CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (_currentValue != value)
                {
                    _currentValue = value;
                    RaisePropertyChanged();
                }
            }
        }

        public BarcodeReaderOptions Options { get; } = new BarcodeReaderOptions
        {
            AutoRotate = false,
            Formats = BarcodeFormat.QrCode,
            Multiple = false,
            TryHarder = false
        };

        public BarcodeFormat Format => BarcodeFormat.QrCode;

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
            // update preview
            CurrentValue = results.First().Value;
        }

        private async Task ImportAsync()
        {
            // disable scanning
            ScanningEnabled = false;

            try
            {
                await _creationManager.ImportCreationPayloadAsync(CurrentValue);

                await _dialogService.ShowMessageBoxAsync(
                    Translate("Import"),
                    Translate("CreationImported"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }
            catch
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Error"),
                    Translate("FailedToImportCreation"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }

            // clear imported code and enable scanning
            CurrentValue = default;
            ScanningEnabled = true;
        }
    }
}
