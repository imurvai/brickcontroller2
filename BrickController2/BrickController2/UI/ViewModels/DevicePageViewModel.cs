using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.Services.UIThread;
using Device = BrickController2.DeviceManagement.Device;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels
{
    public class DevicePageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IUIThreadService _uIThreadService;

        private CancellationTokenSource _connectionTokenSource;
        private Task _connectionTask;
        private bool _reconnect = false;
        private CancellationTokenSource _disappearingTokenSource;
        private bool _isDisappearing = false;

        public DevicePageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IUIThreadService uIThreadService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _uIThreadService = uIThreadService;

            Device = parameters.Get<Device>("device");

            ChannelOutputs = new ObservableCollection<ChannelOutputViewModel>(Device.RegisteredPorts.Select(port => new ChannelOutputViewModel(Device, port)));

            RenameCommand = new SafeCommand(async () => await RenameDeviceAsync());
            BuWizzOutputLevelChangedCommand = new SafeCommand<int>(outputLevel => SetBuWizzOutputLevel(outputLevel));
            BuWizz2OutputLevelChangedCommand = new SafeCommand<int>(outputLevel => SetBuWizzOutputLevel(outputLevel));
        }

        public Device Device { get; }
        public bool IsBuWizzDevice => Device.DeviceType == DeviceType.BuWizz;
        public bool IsBuWizz2Device => Device.DeviceType == DeviceType.BuWizz2;

        public ObservableCollection<ChannelOutputViewModel> ChannelOutputs { get; }

        public ICommand RenameCommand { get; }
        public ICommand BuWizzOutputLevelChangedCommand { get; }
        public ICommand BuWizz2OutputLevelChangedCommand { get; }

        public int BuWizzOutputLevel { get; set; } = 1;
        public int BuWizz2OutputLevel { get; set; } = 1;

        public override async void OnAppearing()
        {
            _isDisappearing = false;
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();

            if (Device.DeviceType != DeviceType.Infrared)
            {
                if (!_deviceManager.IsBluetoothOn)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("TurnOnBluetoothToConnect"),
                        Translate("Ok"),
                        _disappearingTokenSource.Token);

                    await NavigationService.NavigateBackAsync();
                    return;
                }
            }

            _connectionTask = ConnectAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;
            _disappearingTokenSource.Cancel();

            if (_connectionTokenSource != null)
            {
                _connectionTokenSource.Cancel();
                await _connectionTask;
            }

            await Device.DisconnectAsync();
        }

        private async Task RenameDeviceAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Translate("Rename"),
                    Translate("EnterDeviceName"),
                    Device.Name,
                    Translate("DeviceName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("DeviceNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await Device.RenameDeviceAsync(Device, result.Result),
                        Translate("Renaming"));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ConnectAsync()
        {
            _connectionTokenSource = new CancellationTokenSource();
            DeviceConnectionResult connectionResult = DeviceConnectionResult.Ok;

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    using (token.Register(() => _connectionTokenSource?.Cancel()))
                    {
                        connectionResult = await Device.ConnectAsync(
                            _reconnect,
                            OnDeviceDisconnected,
                            Enumerable.Empty<ChannelConfiguration>(),
                            true,
                            true,
                            _connectionTokenSource.Token);
                    }
                },
                Translate("Connecting"),
                null,
                Translate("Cancel"));

            _connectionTokenSource.Dispose();
            _connectionTokenSource = null;

            if (Device.DeviceState == DeviceState.Connected)
            {
                _reconnect = true;

                if (Device.DeviceType == DeviceType.BuWizz)
                {
                    SetBuWizzOutputLevel(BuWizzOutputLevel);
                }
                else if (Device.DeviceType == DeviceType.BuWizz2)
                {
                    SetBuWizzOutputLevel(BuWizz2OutputLevel);
                }
            }
            else
            {
                if (!_isDisappearing)
                {
                    if (connectionResult == DeviceConnectionResult.Error)
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("FailedToConnect"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);
                    }

                    await NavigationService.NavigateBackAsync();
                }
            }
        }

        private void OnDeviceDisconnected(Device device)
        {
            _uIThreadService.RunOnMainThread(() =>
            {
                if (!_isDisappearing)
                {
                    _connectionTask = ConnectAsync();
                }
            });
        }

        private void SetBuWizzOutputLevel(int level)
        {
            Device.SetOutputLevel(level);
        }
    }
}
