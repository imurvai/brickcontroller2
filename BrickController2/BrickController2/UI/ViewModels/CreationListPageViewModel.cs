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

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _disappearingTokenSource;
        private bool _isLoaded;

        public CreationListPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            OpenSettingsPageCommand = new SafeCommand(async () => await OpenSettingsPageAsync());
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
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("BluetoothDevicesWillNOTBeAvailable"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }
        }

        private async Task LoadCreationsAndDevicesAsync()
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
                    Translate("Loading"));
            }
        }

        private async Task OpenSettingsPageAsync()
        {
            await _dialogService.ShowMessageBoxAsync("Pukk", "Here comes settings", "Ok", _disappearingTokenSource.Token);
        }

        private async Task AddCreationAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Translate("Creation"),
                    Translate("EnterCreationName"),
                    null,
                    Translate("CreationName"),
                    Translate("Create"),
                    Translate("Cancel"),
                    KeyboardType.Text,
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
                        Translate("Creating"));

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
                        Translate("Deleting"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}