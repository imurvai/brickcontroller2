using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using System.Threading;
using System;
using BrickController2.UI.Services.Translation;
using Xamarin.Essentials;
using BrickController2.UI.Services.Preferences;
using BrickController2.PlatformServices.SharedFileStorage;
using System.IO;
using System.Linq;

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IPreferencesService _preferencesService;
        private readonly ISharedFileStorageService _sharedFileStorageService;

        private CancellationTokenSource _disappearingTokenSource;
        private bool _isLoaded;

        public CreationListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IPreferencesService preferencesService,
            ISharedFileStorageService sharedFileStorageService)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _preferencesService = preferencesService;
            _sharedFileStorageService = sharedFileStorageService;

            ImportCreationCommand = new SafeCommand(async () => await ImportCreationAsync(), () => _sharedFileStorageService.IsSharedStorageAvailable);
            OpenSettingsPageCommand = new SafeCommand(async () => await navigationService.NavigateToAsync<SettingsPageViewModel>(), () => !_dialogService.IsDialogOpen);
            AddCreationCommand = new SafeCommand(async () => await AddCreationAsync());
            CreationTappedCommand = new SafeCommand<Creation>(async creation => await NavigationService.NavigateToAsync<CreationPageViewModel>(new NavigationParameters(("creation", creation))));
            DeleteCreationCommand = new SafeCommand<Creation>(async creation => await DeleteCreationAsync(creation));
            NavigateToDevicesCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<DeviceListPageViewModel>());
            NavigateToControllerTesterCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<ControllerTesterPageViewModel>());
            NavigateToSequencesCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<SequenceListPageViewModel>());
            NavigateToAboutCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<AboutPageViewModel>());
        }

        public ObservableCollection<Creation> Creations => _creationManager.Creations;

        public ICommand OpenSettingsPageCommand { get; }
        public ICommand AddCreationCommand { get; }
        public ICommand CreationTappedCommand { get; }
        public ICommand DeleteCreationCommand { get; }
        public ICommand ImportCreationCommand { get; }
        public ICommand NavigateToDevicesCommand { get; }
        public ICommand NavigateToControllerTesterCommand { get; }
        public ICommand NavigateToSequencesCommand { get; }
        public ICommand NavigateToAboutCommand { get; }

        public override async void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();

            await RequestPermissionsAsync();
            await LoadCreationsAndDevicesAsync();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource.Cancel();
        }

        private async Task RequestPermissionsAsync()
        {
            try
            {
                var locationPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (locationPermissionStatus != PermissionStatus.Granted)
                {
                    locationPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                _disappearingTokenSource.Token.ThrowIfCancellationRequested();

                if (locationPermissionStatus != PermissionStatus.Granted)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("BluetoothDevicesWillNOTBeAvailable"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);
                }

                _disappearingTokenSource.Token.ThrowIfCancellationRequested();

                var storageReadPermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (storageReadPermissionStatus != PermissionStatus.Granted)
                {
                    storageReadPermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                _disappearingTokenSource.Token.ThrowIfCancellationRequested();

                var storageWritePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (storageWritePermissionStatus != PermissionStatus.Granted)
                {
                    storageWritePermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
                }

                _disappearingTokenSource.Token.ThrowIfCancellationRequested();

                _sharedFileStorageService.IsPermissionGranted = storageReadPermissionStatus == PermissionStatus.Granted && storageWritePermissionStatus == PermissionStatus.Granted;

                if (!_sharedFileStorageService.IsSharedStorageAvailable)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ProfileLoadSaveWillNotBeAvailable"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ImportCreationAsync()
        {
            try
            {
                var creationFiles = Directory.EnumerateFiles(_sharedFileStorageService.SharedStorageDirectory, "*.bc2c", SearchOption.TopDirectoryOnly);
                if (creationFiles?.Any() ?? false)
                {
                    var result = await _dialogService.ShowSelectionDialogAsync(
                        creationFiles,
                        Translate("Creations"),
                        Translate("Cancel"),
                        _disappearingTokenSource.Token);

                    if (result.IsOk)
                    {
                        try
                        {
                            await _creationManager.ImportCreationAsync(result.SelectedItem);
                        }
                        catch (Exception)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Error"),
                                Translate("FailedToImportCreation"),
                                Translate("Ok"),
                                _disappearingTokenSource.Token);
                        }
                    }
                }
                else
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Information"),
                        Translate("NoCreationsToImport"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task LoadCreationsAndDevicesAsync()
        {
            try
            {
                if (!_isLoaded)
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            await _creationManager.LoadCreationsAndSequencesAsync();
                            await _deviceManager.LoadDevicesAsync();
                            _isLoaded = true;
                        },
                        Translate("Loading"),
                        token: _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task AddCreationAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    null,
                    Translate("CreationName"),
                    Translate("Create"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (creationName) => !string.IsNullOrEmpty(creationName),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("CreationNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    Creation creation = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            creation = await _creationManager.AddCreationAsync(result.Result);
                            await _creationManager.AddControllerProfileAsync(creation, Translate("DefaultProfile"));
                        },
                        Translate("Creating"),
                        token: _disappearingTokenSource.Token);

                    await NavigationService.NavigateToAsync<CreationPageViewModel>(new NavigationParameters(("creation", creation)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteCreationAsync(Creation creation)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteCreation")} '{creation.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteCreationAsync(creation),
                        Translate("Deleting"),
                        token: _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}