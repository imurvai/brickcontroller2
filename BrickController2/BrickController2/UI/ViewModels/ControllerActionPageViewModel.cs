using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.Preferences;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
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
        private readonly IPreferences _preferences;

        private CancellationTokenSource _disappearingTokenSource;

        private Device _selectedDevice;

        public ControllerActionPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IPreferences preferences,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _deviceManager = deviceManager;
            _dialogService = dialogService;
            _preferences = preferences;

            ControllerAction = parameters.Get<ControllerAction>("controlleraction", null);
            ControllerEvent = parameters.Get<ControllerEvent>("controllerevent", null) ?? ControllerAction?.ControllerEvent;

            var device = _deviceManager.GetDeviceById(ControllerAction?.DeviceId);
            if (ControllerAction != null && device != null)
            {
                SelectedDevice = device;
                Action.Channel = ControllerAction.Channel;
                Action.IsInvert = ControllerAction.IsInvert;
                Action.ChannelOutputType = ControllerAction.ChannelOutputType;
                Action.MaxServoAngle = ControllerAction.MaxServoAngle;
                Action.ButtonType = ControllerAction.ButtonType;
                Action.AxisType = ControllerAction.AxisType;
                Action.AxisCharacteristic = ControllerAction.AxisCharacteristic;
                Action.MaxOutputPercent = ControllerAction.MaxOutputPercent;
                Action.AxisDeadZonePercent = ControllerAction.AxisDeadZonePercent;
                Action.ServoBaseAngle = ControllerAction.ServoBaseAngle;
                Action.StepperAngle = ControllerAction.StepperAngle;
            }
            else
            {
                var lastSelectedDeviceId = _preferences.Get<string>("LastSelectedDeviceId", null, "com.scn.BrickController2.ControllerActionPage");
                SelectedDevice = _deviceManager.GetDeviceById(lastSelectedDeviceId) ?? _deviceManager.Devices.FirstOrDefault();
                Action.Channel = 0;
                Action.IsInvert = false;
                Action.ChannelOutputType = ChannelOutputType.NormalMotor;
                Action.MaxServoAngle = 90;
                Action.ButtonType = ControllerButtonType.Normal;
                Action.AxisType = ControllerAxisType.Normal;
                Action.AxisCharacteristic = ControllerAxisCharacteristic.Linear;
                Action.MaxOutputPercent = 100;
                Action.AxisDeadZonePercent = 0;
                Action.ServoBaseAngle = 0;
                Action.StepperAngle = 90;
            }

            SaveControllerActionCommand = new SafeCommand(async () => await SaveControllerActionAsync(), () => SelectedDevice != null);
            DeleteControllerActionCommand = new SafeCommand(async () => await DeleteControllerActionAsync());
            OpenDeviceDetailsCommand = new SafeCommand(async () => await OpenDeviceDetailsAsync(), () => SelectedDevice != null);
            OpenChannelSetupCommand = new SafeCommand(async () => await OpenChannelSetupAsync(), () => SelectedDevice != null);
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
                Action.DeviceId = value.Id;

                if (_selectedDevice.NumberOfChannels <= Action.Channel)
                {
                    Action.Channel = 0;
                }

                RaisePropertyChanged();
            }
        }

        public ControllerAction Action { get; } = new ControllerAction();

        public ICommand SaveControllerActionCommand { get; }
        public ICommand DeleteControllerActionCommand { get; }
        public ICommand OpenDeviceDetailsCommand { get; }
        public ICommand OpenChannelSetupCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _preferences.Set<string>("LastSelectedDeviceId", _selectedDevice.Id, "com.scn.BrickController2.ControllerActionPage");

            _disappearingTokenSource?.Cancel();
        }

        private async Task SaveControllerActionAsync()
        {
            if (SelectedDevice == null)
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("SelectDeviceBeforeSaving"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
                return;
            }

            await _dialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    if (ControllerAction != null)
                    {
                        await _creationManager.UpdateControllerActionAsync(
                            ControllerAction,
                            Action.DeviceId,
                            Action.Channel,
                            Action.IsInvert,
                            Action.ButtonType,
                            Action.AxisType,
                            Action.AxisCharacteristic,
                            Action.MaxOutputPercent,
                            Action.AxisDeadZonePercent,
                            Action.ChannelOutputType,
                            Action.MaxServoAngle,
                            Action.ServoBaseAngle,
                            Action.StepperAngle);
                    }
                    else
                    {
                        await _creationManager.AddOrUpdateControllerActionAsync(
                            ControllerEvent,
                            Action.DeviceId,
                            Action.Channel,
                            Action.IsInvert,
                            Action.ButtonType,
                            Action.AxisType,
                            Action.AxisCharacteristic,
                            Action.MaxOutputPercent,
                            Action.AxisDeadZonePercent,
                            Action.ChannelOutputType,
                            Action.MaxServoAngle,
                            Action.ServoBaseAngle,
                            Action.StepperAngle);
                    }
                },
                Translate("Saving"));

            await NavigationService.NavigateBackAsync();
        }

        private async Task DeleteControllerActionAsync()
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    Translate("AreYouSureToDeleteControllerAction"),
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    if (ControllerAction != null)
                    {
                        await _dialogService.ShowProgressDialogAsync(
                            false,
                            async (progressDialog, token) => await _creationManager.DeleteControllerActionAsync(ControllerAction),
                            Translate("Deleting"));
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

        private async Task OpenChannelSetupAsync()
        {
            if (SelectedDevice == null)
            {
                return;
            }

            await NavigationService.NavigateToAsync<ChannelSetupPageViewModel>(new NavigationParameters(("device", SelectedDevice), ("controlleraction", Action)));
        }
    }
}
