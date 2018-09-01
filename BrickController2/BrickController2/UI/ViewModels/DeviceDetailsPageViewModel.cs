using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using Xamarin.Forms;
using Device = BrickController2.DeviceManagement.Device;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using BrickController2.Helpers;
using System.Collections.ObjectModel;
using System.Threading;

namespace BrickController2.UI.ViewModels
{
    public class DeviceDetailsPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _connectCancellationTokenSource;

        public DeviceDetailsPageViewModel(
            INavigationService navigationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            Device = parameters.Get<Device>("device");

            MenuCommand = new Command(async () => await SelectMenuItem());
            ConnectCommand = new Command(async () => await ConnectAsync());

            for (int i = 0; i < Device.NumberOfChannels; i++)
            {
                Outputs.Add(new DeviceOutputViewModel(Device, i));
            }
        }

        public Device Device { get; }

        public ICommand MenuCommand { get; }
        public ICommand ConnectCommand { get; }

        public ObservableCollection<DeviceOutputViewModel> Outputs { get; } = new ObservableCollection<DeviceOutputViewModel>();

        public override void OnAppearing()
        {
            Device.DeviceStateChanged += DeviceStateChangedHandler;

            base.OnAppearing();
        }

        public override async void OnDisappearing()
        {
            Device.DeviceStateChanged -= DeviceStateChangedHandler;

            _connectCancellationTokenSource?.Cancel();
            _connectCancellationTokenSource?.Dispose();
            _connectCancellationTokenSource = null;

            await Device.DisconnectAsync();

            base.OnDisappearing();
        }

        private async Task SelectMenuItem()
        {
            var menuActions = new Dictionary<string, Func<Task>>
            {
                { "Rename device", RenameDeviceAsync },
                { "Delete device", DeleteDeviceAsync }
            };

            var selectedItem = await DisplayActionSheetAsync("Select an option", "Cancel", null, menuActions.GetKeyArray());
            if (menuActions.ContainsKey(selectedItem))
            {
                await menuActions[selectedItem].Invoke();
            }
        }

        private async Task RenameDeviceAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync("Rename", "Enter a new name for the device", Device.Name, "Device name", "Rename", "Cancel");
            if (result.IsOk)
            {
                if (string.IsNullOrWhiteSpace(result.Result))
                {
                    await DisplayAlertAsync("Warning", "Device name can not be empty.", "Ok");
                    return;
                }

                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await Device.RenameDeviceAsync(Device, result.Result),
                    "Renaming...");
            }
        }

        private async Task DeleteDeviceAsync()
        {
            if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this device?", "Yes", "No"))
            {
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    async (progressDialog, token) => await _deviceManager.DeleteDeviceAsync(Device),
                    "Deleting...");

                await NavigationService.NavigateBackAsync();
            }
        }

        private async Task ConnectAsync()
        {
            _connectCancellationTokenSource = new CancellationTokenSource();

            DeviceConnectionResult connectionSuccess = DeviceConnectionResult.Ok;
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    connectionSuccess = await Device.ConnectAsync(token);
                },
                "Connecting...",
                null,
                "Cancel",
                _connectCancellationTokenSource);

            _connectCancellationTokenSource.Dispose();
            _connectCancellationTokenSource = null;

            if (connectionSuccess == DeviceConnectionResult.Error)
            {
                await DisplayAlertAsync("Warning", "Failed to connect to device.", "Ok");
            }
        }

        private async Task DisconnectAsync()
        {

        }

        private async void DeviceStateChangedHandler(object sender, DeviceStateChangedEventArgs args)
        {
            if (args.OldState == DeviceState.Connected && args.NewState == DeviceState.Disconnected && args.IsError)
            {
                var result = await _dialogService.ShowQuestionDialogAsync("Device connection lost", "Do you want to reconnect?", "Yes", "No");
                if (result)
                {
                    await ConnectAsync();
                }
            }
        }
    }
}
