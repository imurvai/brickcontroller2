using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels
{
    public class ChannelSetupPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _connectionTokenSource;
        private Task _connectionTask;
        private CancellationTokenSource _disappearingTokenSource;
        private bool _isDisappearing = false;

        private int _servoBaseAngle;

        public ChannelSetupPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            Device = parameters.Get<Device>("device");
            Action = parameters.Get<ControllerAction>("controlleraction");
            ServoBaseAngle = Action.ServoBaseAngle;

            SaveChannelSettingsCommand = new SafeCommand(async () => await SaveChannelSettingsAsync(), () => !_dialogService.IsDialogOpen);
            AutoCalibrateServoCommand = new SafeCommand(async () => await AutoCalibrateServoAsync(), () => Device.CanAutoCalibrateOutput);
            ResetServoBaseCommand = new SafeCommand(async () => await ResetServoBaseAngleAsync(), () => Device.CanResetOutput);
        }

        public Device Device { get; }
        public ControllerAction Action { get; }

        public int ServoBaseAngle
        {
            get { return _servoBaseAngle; }
            set { _servoBaseAngle = value; RaisePropertyChanged(); }
        }

        public ICommand SaveChannelSettingsCommand { get; }
        public ICommand AutoCalibrateServoCommand { get; }
        public ICommand ResetServoBaseCommand { get; }

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

                    if (!_isDisappearing)
                    {
                        await NavigationService.NavigateBackAsync();
                    }
                    return;
                }
            }

            _connectionTokenSource = new CancellationTokenSource();
            _connectionTask = ConnectAsync();
        }

        public override async void OnDisappearing()
        {
            _isDisappearing = true;
            _disappearingTokenSource.Cancel();

            if (_connectionTokenSource != null && _connectionTask != null)
            {
                _connectionTokenSource.Cancel();
                await _connectionTask;
            }

            await Device.DisconnectAsync();
        }

        private async Task ConnectAsync()
        {
            while (!_connectionTokenSource.IsCancellationRequested)
            {
                if (Device.DeviceState != DeviceState.Connected)
                {
                    var connectionResult = DeviceConnectionResult.Ok;

                    var dialogResult = await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) =>
                        {
                            using (token.Register(() => _connectionTokenSource.Cancel()))
                            {
                                await Device.ConnectAsync(
                                    false,
                                    OnDeviceDisconnected,
                                    Enumerable.Empty<ChannelConfiguration>(),
                                    false,
                                    false,
                                    token);
                            }
                        },
                        Translate("ConnectingTo"),
                        Device.Name,
                        Translate("Cancel"),
                        _connectionTokenSource.Token);

                    if (dialogResult.IsCancelled)
                    {
                        await Device.DisconnectAsync();

                        if (!_isDisappearing)
                        {
                            await NavigationService.NavigateBackAsync();
                        }

                        return;
                    }
                    else
                    {
                        if (connectionResult == DeviceConnectionResult.Error)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Warning"),
                                Translate("FailedToConnect"),
                                Translate("Ok"),
                                _disappearingTokenSource.Token);

                            if (!_isDisappearing)
                            {
                                await NavigationService.NavigateBackAsync();
                            }

                            return;
                        }
                    }
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        private void OnDeviceDisconnected(Device device)
        {
        }

        private async Task SaveChannelSettingsAsync()
        {
            Action.ServoBaseAngle = ServoBaseAngle;
            await NavigationService.NavigateModalBackAsync();
        }

        private async Task AutoCalibrateServoAsync()
        {
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    var result = await Device.AutoCalibrateOutputAsync(Action.Channel, token);
                    if (result.Success)
                    {
                        ServoBaseAngle = (int)(result.BaseServoAngle * 180);
                    }
                },
                Translate("Calibrating"),
                null,
                null,
                _disappearingTokenSource.Token);
        }

        private async Task ResetServoBaseAngleAsync()
        {
            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    await Device.ResetOutputAsync(Action.Channel, ServoBaseAngle / 180F, token);
                },
                Translate("Reseting"),
                null,
                null,
                _disappearingTokenSource.Token);
        }
    }
}
