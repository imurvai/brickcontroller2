using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private bool _isLoaded;

        public CreationListPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            AddCreationCommand = new SafeCommand(async () => await AddCreation());
            CreationTappedCommand = new SafeCommand<Creation>(async creation => await NavigationService.NavigateToAsync<CreationPageViewModel>(new NavigationParameters(("creation", creation))));
            NavigateToDevicesCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<DeviceListPageViewModel>());
            NavigateToControllerTesterCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<ControllerTesterPageViewModel>());
            NavigateToAboutCommand = new SafeCommand(async () => await NavigationService.NavigateToAsync<AboutPageViewModel>());
        }

        public ObservableCollection<Creation> Creations => _creationManager.Creations;

        public ICommand AddCreationCommand { get; }
        public ICommand CreationTappedCommand { get; }
        public ICommand NavigateToDevicesCommand { get; }
        public ICommand NavigateToControllerTesterCommand { get; }
        public ICommand NavigateToAboutCommand { get; }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            await RequestPermissions();
            await LoadCreationsAndDevices();
        }

        private async Task RequestPermissions()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
            if (status != PermissionStatus.Granted)
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                {
                    await DisplayAlertAsync("Permission request", "Location permission is needed for accessing bluetooth", "Ok");
                }

                var result = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                if (result.ContainsKey(Permission.Location))
                {
                    status = result[Permission.Location];
                }
            }

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlertAsync("Warning", "Bluetooth devices will NOT be available.", "Ok");
            }
        }

        private async Task LoadCreationsAndDevices()
        {
            if (!_isLoaded)
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) =>
                    {
                        await _creationManager.LoadCreationsAsync();
                        await _deviceManager.LoadDevicesAsync();
                        _isLoaded = true;
                    },
                    "Loading...");
            }
        }

        private async Task AddCreation()
        {
            var result = await _dialogService.ShowInputDialogAsync("Creation", "Enter a creation name", null, "Creation name", "Create", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Creation name can not be empty.", "Ok");
                    return;
                }

                Creation creation = null;
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) =>
                    {
                        creation = await _creationManager.AddCreationAsync(result.Result);
                        await _creationManager.AddControllerProfileAsync(creation, "Default profile");
                    },
                    "Creating...");

                await NavigationService.NavigateToAsync<CreationPageViewModel>(new NavigationParameters(("creation", creation)));
            }
        }
    }
}