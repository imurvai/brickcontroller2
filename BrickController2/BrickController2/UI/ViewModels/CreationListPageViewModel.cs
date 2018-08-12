using System.Collections.ObjectModel;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace BrickController2.UI.ViewModels
{
    public class CreationListPageViewModel : PageViewModelBase
    {
        private readonly ICreationRepository _creationRepository;

        public CreationListPageViewModel(INavigationService navigationService, ICreationRepository creationRepository)
            : base(navigationService)
        {
            _creationRepository = creationRepository;

            AddCreationCommand = new Command(async () =>
            {
                var result = await DisplayActionSheetAsync("Choose...", "Cancel", "Destruction", "1", "2", "3");
                await DisplayAlertAsync("Result", result, "Ok");
            });

            NavigeteToDeviceListCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<DeviceListPageViewModel>(null);
            });

            NavigateToControllerTesterCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<ControllerTesterPageViewModel>();
            });
        }

        public ICommand AddCreationCommand { get; }
        public ICommand SelectNavigationTargetCommand { get; }
        public ICommand NavigeteToDeviceListCommand { get; }
        public ICommand NavigateToControllerTesterCommand { get; }

        public ObservableCollection<Creation> Creations { get; } = new ObservableCollection<Creation>();

        public override async void OnAppearing()
        {
            base.OnAppearing();

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

            var creations = await _creationRepository.GetCreationsAsync();
            Creations.Clear();
            foreach (var creation in creations)
            {
                Creations.Add(creation);
            }
        }
    }
}