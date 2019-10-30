using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.Helpers;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.Services.UIThread;

namespace BrickController2.UI.ViewModels
{
    public class ChannelSetupPageViewModel : PageViewModelBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IUIThreadService _uIThreadService;

        private CancellationTokenSource _tokenSource;
        private Task _connectionTask;
        private bool _reconnect = false;
        private CancellationTokenSource _disappearingTokenSource;
        private bool _isDisappearing = false;

        private int _servoBaseAngle;

        public ChannelSetupPageViewModel(
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
            Action = parameters.Get<ControllerAction>("controlleraction");
            ServoBaseAngle = Action.ServoBaseAngle;
            ChannelName = Device.GetChannelName(Action.Channel, TranslationService);

            SaveChannelSettingsCommand = new SafeCommand(async () => await SaveChannelSettingsAsync());
            AutoCalibrateServoCommand = new SafeCommand(async () => await AutoCalibrateServoAsync());
            ResetServoBaseCommand = new SafeCommand(async () => await ResetServoBaseAngleAsync());
        }

        public Device Device { get; }
        public ControllerAction Action { get; }

        public string ChannelName { get; } 

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

            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                await _connectionTask;
            }

            await Device.DisconnectAsync();
        }

        private async Task ConnectAsync()
        {
            _tokenSource = new CancellationTokenSource();
            DeviceConnectionResult connectionResult = DeviceConnectionResult.Ok;

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    using (token.Register(() => _tokenSource.Cancel()))
                    {
                        connectionResult = await Device.ConnectAsync(
                            _reconnect,
                            OnDeviceDisconnected,
                            Enumerable.Empty<ChannelConfiguration>(),
                            false,
                            false,
                            _tokenSource.Token);
                    }
                },
                Translate("Connecting"),
                null,
                Translate("Cancel"));

            _tokenSource.Dispose();
            _tokenSource = null;

            if (Device.DeviceState == DeviceState.Connected)
            {
                _reconnect = true;
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

        private async Task SaveChannelSettingsAsync()
        {
            Action.ServoBaseAngle = ServoBaseAngle;
            await NavigationService.NavigateModalBackAsync();
        }

        private async Task AutoCalibrateServoAsync()
        {
            _tokenSource = new CancellationTokenSource();

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    using (token.Register(() => _tokenSource.Cancel()))
                    {
                        var result = await Device.AutoCalibrateOutputAsync(Action.Channel, _tokenSource.Token);
                        if (result.Success)
                        {
                            ServoBaseAngle = (int)(result.BaseServoAngle * 180);
                        }
                    }
                },
                Translate("Calibrating"),
                null,
                null);
        }

        private async Task ResetServoBaseAngleAsync()
        {
            _tokenSource = new CancellationTokenSource();

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    using (token.Register(() => _tokenSource.Cancel()))
                    {
                        await Device.ResetOutputAsync(Action.Channel, ServoBaseAngle / 180F, _tokenSource.Token);
                    }
                },
                Translate("Reseting"),
                null,
                null);
        }
    }
}
