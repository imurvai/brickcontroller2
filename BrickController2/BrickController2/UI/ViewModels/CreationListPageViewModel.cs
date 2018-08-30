using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using BrickController2.UI.Services;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private bool _isLoaded = false;

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

            AddCreationCommand = new Command(async () => await AddCreation());
            CreationTappedCommand = new Command<Creation>(async creation => await NavigationService.NavigateToAsync<CreationDetailsPageViewModel>(new NavigationParameters(("creation", creation))));
            NavigateToDevicesCommand = new Command(async () => await NavigationService.NavigateToAsync<DeviceListPageViewModel>());
            NavigateToControllerTesterCommand = new Command(async () => await NavigationService.NavigateToAsync<ControllerTesterPageViewModel>());
            NavigateToAboutCommand = new Command(async () => await DisplayAlertAsync(null, "About selected", "Ok"));
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
                using (_dialogService.ShowProgressDialog(false, "Loading..."))
                {
                    await _creationManager.LoadCreationsAsync();
                    await _deviceManager.LoadDevicesAsync();
                    await Task.Delay(5000);
                    _isLoaded = true;
                }
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

                Creation creation;
                using (_dialogService.ShowProgressDialog(false, "Creating..."))
                {
                    creation = await _creationManager.AddCreationAsync(result.Result);
                }

                await NavigationService.NavigateToAsync<CreationDetailsPageViewModel>(new NavigationParameters(("creation", creation)));
            }
        }
    }
}