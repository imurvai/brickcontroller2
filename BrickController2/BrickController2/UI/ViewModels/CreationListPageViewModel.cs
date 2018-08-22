using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Navigation;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IUserDialogs _userDialogs;
        private bool _isLoaded = false;

        public CreationListPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IUserDialogs userDialogs)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _userDialogs = userDialogs;

            AddCreationCommand = new Command(async () =>
            {
                var result = await _userDialogs.PromptAsync("New creation name", null, "Create", "Cancel", "Name", InputType.Default, null);
                if (result.Ok)
                {
                    if (string.IsNullOrWhiteSpace(result.Text))
                    {
                        await DisplayAlertAsync("Warning", "Creation name can not be empty.", "Ok");
                        return;
                    }

                    var progressConfig = new ProgressDialogConfig()
                        .SetIsDeterministic(false)
                        .SetTitle("Creating...");

                    Creation creation;
                    using (_userDialogs.Progress(progressConfig))
                    {
                        creation = await _creationManager.AddCreationAsync(result.Text);
                    }

                    await NavigationService.NavigateToAsync<CreationDetailsPageViewModel>(new NavigationParameters(("creation", creation)));
                }
            });

            MenuCommand = new Command(async () =>
            {
                var result = await DisplayActionSheetAsync("Select option", "Cancel", null, "Devices", "Controller tester", "About");
                switch (result)
                {
                    case "Devices":
                        await NavigationService.NavigateToAsync<DeviceListPageViewModel>();
                        break;

                    case "Controller tester":
                        await NavigationService.NavigateToAsync<ControllerTesterPageViewModel>();
                        break;

                    case "About":
                        await DisplayAlertAsync(null, "About selected", "Ok");
                        break;
                }
            });

            CreationTappedCommand = new Command(async creation =>
            {
                await NavigationService.NavigateToAsync<CreationDetailsPageViewModel>(new NavigationParameters(("creation", creation)));
            });
        }

        public ObservableCollection<Creation> Creations => _creationManager.Creations;

        public ICommand AddCreationCommand { get; }
        public ICommand MenuCommand { get; }
        public ICommand CreationTappedCommand { get; }

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
                var progressDialogConfig = new ProgressDialogConfig()
                    .SetTitle("Loading...")
                    .SetIsDeterministic(false);

                using (_userDialogs.Progress(progressDialogConfig))
                {
                    await _creationManager.LoadCreationsAsync();
                    await _deviceManager.LoadDevicesAsync();
                    _isLoaded = true;
                }
            }
        }
    }
}