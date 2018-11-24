using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class ControllerActionPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _disappearingTokenSource;

        private Device _selectedDevice;
        private int _channel;
        private ChannelOutputType _channelOutputType;
        private bool _isInvert;
        private ControllerButtonType _buttonType;
        private ControllerAxisCharacteristic _axisCharacteristic;
        private int _maxOutputPercent;
        private int _axisDeadZonePercent;
        private int _maxServoAngle;

        public ControllerActionPageViewModel(
            INavigationService navigationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            NavigationParameters parameters)
            : base(navigationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;

            ControllerAction = parameters.Get<ControllerAction>("controlleraction", null);
            ControllerEvent = parameters.Get<ControllerEvent>("controllerevent", null) ?? ControllerAction.ControllerEvent;

            var device = _deviceManager.GetDeviceById(ControllerAction?.DeviceId);
            if (ControllerAction != null && device != null)
            {
                SelectedDevice = device;
                Channel = ControllerAction.Channel;
                IsInvert = ControllerAction.IsInvert;
                ChannelOutputType = ControllerAction.ChannelOutputType;
                MaxServoAngle = ControllerAction.MaxServoAngle;
                ButtonType = ControllerAction.ButtonType;
                AxisCharacteristic = ControllerAction.AxisCharacteristic;
                MaxOutputPercent = ControllerAction.MaxOutputPercent;
                AxisDeadZonePercent = ControllerAction.AxisDeadZonePercent;
            }
            else
            {
                SelectedDevice = _deviceManager.Devices.FirstOrDefault();
                Channel = 0;
                IsInvert = false;
                ChannelOutputType = ChannelOutputType.NormalMotor;
                MaxServoAngle = 90;
                ButtonType = ControllerButtonType.Normal;
                AxisCharacteristic = ControllerAxisCharacteristic.Linear;
                MaxOutputPercent = 100;
                AxisDeadZonePercent = 0;
            }

            SaveControllerActionCommand = new SafeCommand(async () => await SaveControllerActionAsync(), () => SelectedDevice != null);
            DeleteControllerActionCommand = new SafeCommand(async () => await DeleteControllerActionAsync());
            OpenDeviceDetailsCommand = new SafeCommand(async () => await OpenDeviceDetailsAsync(), () => SelectedDevice != null);
        }

        public ObservableCollection<Device> Devices => _deviceManager.Devices;

        public ControllerEvent ControllerEvent { get; }
        public ControllerAction ControllerAction { get; }

        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                _selectedDevice = value;
                if (_selectedDevice.NumberOfChannels <= Channel)
                {
                    Channel = 0;
                }

                RaisePropertyChanged();
            }
        }

        public int Channel
        {
            get { return _channel; }
            set { _channel = value; RaisePropertyChanged(); }
        }

        public bool IsInvert
        {
            get { return _isInvert; }
            set { _isInvert = value; RaisePropertyChanged(); }
        }

        public int MaxOutputPercent
        {
            get { return _maxOutputPercent; }
            set { _maxOutputPercent = value; RaisePropertyChanged(); }
        }

        public ChannelOutputType ChannelOutputType
        {
            get { return _channelOutputType; }
            set { _channelOutputType = value; RaisePropertyChanged(); }
        }

        public int MaxServoAngle
        {
            get { return _maxServoAngle; }
            set { _maxServoAngle = value; RaisePropertyChanged(); }
        }

        public ControllerButtonType ButtonType
        {
            get { return _buttonType; }
            set { _buttonType = value; RaisePropertyChanged(); }
        }

        public ControllerAxisCharacteristic AxisCharacteristic
        {
            get { return _axisCharacteristic; }
            set { _axisCharacteristic = value; RaisePropertyChanged(); }
        }

        public int AxisDeadZonePercent
        {
            get { return _axisDeadZonePercent; }
            set { _axisDeadZonePercent = value; RaisePropertyChanged(); }
        }

        public ICommand SaveControllerActionCommand { get; }
        public ICommand DeleteControllerActionCommand { get; }
        public ICommand OpenDeviceDetailsCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource?.Cancel();
        }

        private async Task SaveControllerActionAsync()
        {
            if (SelectedDevice == null)
            {
                await _dialogService.ShowMessageBoxAsync("Warning", "Select a device before saving.", "Ok", _disappearingTokenSource.Token);
                return;
            }

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    if (ControllerAction != null)
                    {
                        await _creationManager.UpdateControllerActionAsync(ControllerAction, SelectedDevice.Id, Channel, IsInvert, ButtonType, AxisCharacteristic, MaxOutputPercent, AxisDeadZonePercent);
                    }
                    else
                    {
                        await _creationManager.AddOrUpdateControllerActionAsync(ControllerEvent, SelectedDevice.Id, Channel, IsInvert, ButtonType, AxisCharacteristic, MaxOutputPercent, AxisDeadZonePercent);
                    }
                },
                "Saving...");

            await NavigationService.NavigateBackAsync();
        }

        private async Task DeleteControllerActionAsync()
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync("Confirm", "Are you sure to delete this controller action?", "Yes", "No", _disappearingTokenSource.Token))
                {
                    if (ControllerAction != null)
                    {
                        await _dialogService.ShowProgressDialogAsync(
                            false,
                            async (progressDialog, token) => await _creationManager.DeleteControllerActionAsync(ControllerAction),
                            "Deleting...");
                    }

                    await NavigationService.NavigateBackAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task OpenDeviceDetailsAsync()
        {
            if (SelectedDevice == null)
            {
                return;
            }

            await NavigationService.NavigateToAsync<DevicePageViewModel>(new NavigationParameters(("device", SelectedDevice)));
        }
    }
}
