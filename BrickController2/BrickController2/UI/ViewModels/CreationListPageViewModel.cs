using System.Collections.ObjectModel;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.UI.Navigation;
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

            NavigateToCreationDetailsCommand = new Command(async () =>
            {
                await NavigationService.NavigateToAsync<CreationDetailsPageViewModel>(new NavigationParameters(("key1", "value1"), ("key2", "value2")));
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

        public ICommand NavigateToCreationDetailsCommand { get; }
        public ICommand NavigeteToDeviceListCommand { get; }
        public ICommand NavigateToControllerTesterCommand { get; }

        public ObservableCollection<Creation> Creations { get; } = new ObservableCollection<Creation>();

        public override async void OnAppearing()
        {
            base.OnAppearing();

            var creations = await _creationRepository.GetCreationsAsync();
            Creations.Clear();
            foreach (var creation in creations)
            {
                Creations.Add(creation);
            }
        }
    }
}