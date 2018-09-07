using System.Threading.Tasks;
using BrickController2.CreationManagement;
using BrickController2.UI.Services.Navigation;
using System.Windows.Input;
using BrickController2.UI.Services.Dialog;
using System.Collections.Generic;
using System;
using BrickController2.Helpers;
using BrickController2.UI.Commands;

namespace BrickController2.UI.ViewModels
{
    public class CreationDetailsPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;

        public CreationDetailsPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;

            Creation = parameters.Get<Creation>("creation");

            MenuCommand = new SafeCommand(async () => await SelectMenuItemAsync());
            AddControllerProfileCommand = new SafeCommand(async () => await AddControllerProfileAsync());
            ControllerProfileTappedCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await NavigationService.NavigateToAsync<ControllerProfileDetailsPageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile))));
        }

        public Creation Creation { get; }

        public ICommand MenuCommand { get; }
        public ICommand AddControllerProfileCommand { get; }
        public ICommand ControllerProfileTappedCommand { get; }

        private async Task SelectMenuItemAsync()
        {
            var menuActions = new Dictionary<string, Func<Task>>
            {
                { "Rename creation", RenameCreationAsync },
                { "Delete creation", DeleteCreationAsync }
            };

            var selectedItem = await DisplayActionSheetAsync("Select an option", "Cancel", null, menuActions.GetKeyArray());
            if (menuActions.ContainsKey(selectedItem))
            {
                await menuActions[selectedItem].Invoke();
            }
        }

        private async Task RenameCreationAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new creation name", Creation.Name, "Creation name", "Rename", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Creation name can not be empty.", "Ok");
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.RenameCreationAsync(Creation, result.Result),
                    "Renaming...");
            }
        }

        private async Task DeleteCreationAsync()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this creation?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _creationManager.DeleteCreationAsync(Creation),
                    "Deleting...");

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task AddControllerProfileAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Controller profile", "Enter a profile name", null, "Profile name", "Create", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Controller profile name can not be empty.", "Ok");
                    return;
                }

                ControllerProfile controllerProfile = null;
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => controllerProfile = await _creationManager.AddControllerProfileAsync(Creation, result.Result),
                    "Creating...");

                await NavigationService.NavigateToAsync<ControllerProfileDetailsPageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile)));
            }
        }
    }
}
