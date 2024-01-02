using BrickController2.CreationManagement;
using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.Services.Translation;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels
{
    public class ControllerActionPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDeviceManager _deviceManager;
        private readonly IDialogService _dialogService;
        private readonly IPreferencesService _preferences;

        private CancellationTokenSource _disappearingTokenSource;

        private Device _selectedDevice;

        public ControllerActionPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDeviceManager deviceManager,
            IDialogService dialogService,
            IPreferencesService preferences,
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
                Action.AxisActiveZonePercent = ControllerAction.AxisActiveZonePercent;
                Action.AxisDeadZonePercent = ControllerAction.AxisDeadZonePercent;
                Action.ServoBaseAngle = ControllerAction.ServoBaseAngle;
                Action.StepperAngle = ControllerAction.StepperAngle;
                Action.SequenceName = ControllerAction.SequenceName;
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
                Action.AxisActiveZonePercent = 100;
                Action.AxisDeadZonePercent = 0;
                Action.ServoBaseAngle = 0;
                Action.StepperAngle = 90;
                Action.SequenceName = string.Empty;
            }

            SaveControllerActionCommand = new SafeCommand(async () => await SaveControllerActionAsync(), () => SelectedDevice != null && !_dialogService.IsDialogOpen);
            SelectDeviceCommand = new SafeCommand(async () => await SelectDeviceAsync());
            OpenDeviceDetailsCommand = new SafeCommand(async () => await OpenDeviceDetailsAsync(), () => SelectedDevice != null);
            SelectChannelOutputTypeCommand = new SafeCommand(async () => await SelectChannelOutputTypeAsync(), () => SelectedDevice != null);
            OpenChannelSetupCommand = new SafeCommand(async () => await OpenChannelSetupAsync(), () => SelectedDevice != null);
            SelectButtonTypeCommand = new SafeCommand(async () => await SelectButtonTypeAsync());
            SelectSequenceCommand = new SafeCommand(async () => await SelectSequenceAsync());
            OpenSequenceEditorCommand = new SafeCommand(async () => await OpenSequenceEditorAsync());
            SelectAxisTypeCommand = new SafeCommand(async () => await SelectAxisTypeAsync());
            SelectAxisCharacteristicCommand = new SafeCommand(async () => await SelectAxisCharacteristicAsync());
        }

        public ObservableCollection<Device> Devices => _deviceManager.Devices;
        public ObservableCollection<string> Sequences => new ObservableCollection<string>(_creationManager.Sequences.Select(s => s.Name).ToArray());

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
        public ICommand SelectDeviceCommand { get; }
        public ICommand SelectChannelOutputTypeCommand { get; }
        public ICommand OpenDeviceDetailsCommand { get; }
        public ICommand OpenChannelSetupCommand { get; }
        public ICommand SelectButtonTypeCommand { get; }
        public ICommand SelectSequenceCommand { get; }
        public ICommand OpenSequenceEditorCommand { get; }
        public ICommand SelectAxisTypeCommand { get; }
        public ICommand SelectAxisCharacteristicCommand { get; }

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
                            Action.AxisActiveZonePercent,
                            Action.AxisDeadZonePercent,
                            Action.ChannelOutputType,
                            Action.MaxServoAngle,
                            Action.ServoBaseAngle,
                            Action.StepperAngle,
                            Action.SequenceName);
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
                            Action.AxisActiveZonePercent,
                            Action.AxisDeadZonePercent,
                            Action.ChannelOutputType,
                            Action.MaxServoAngle,
                            Action.ServoBaseAngle,
                            Action.StepperAngle,
                            Action.SequenceName);
                    }
                },
                Translate("Saving"),
                token: _disappearingTokenSource.Token);

            await NavigationService.NavigateBackAsync();
        }

        private async Task SelectDeviceAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Devices,
                Translate("SelectDevice"),
                Translate("Cancel"),
                _disappearingTokenSource.Token);

            if (result.IsOk)
            {
                SelectedDevice = result.SelectedItem;
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

        private async Task SelectChannelOutputTypeAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames(typeof(ChannelOutputType)),
                Translate("ChannelType"),
                Translate("Cancel"),
                _disappearingTokenSource.Token);

            if (result.IsOk)
            {
                Action.ChannelOutputType = (ChannelOutputType)Enum.Parse(typeof(ChannelOutputType), result.SelectedItem);
            }
        }

        private async Task OpenChannelSetupAsync()
        {
            if (SelectedDevice == null)
            {
                return;
            }

            await NavigationService.NavigateToAsync<ChannelSetupPageViewModel>(new NavigationParameters(("device", SelectedDevice), ("controlleraction", Action)));
        }

        private async Task SelectButtonTypeAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames(typeof(ControllerButtonType)),
                Translate("ButtonType"),
                Translate("Cancel"),
                _disappearingTokenSource.Token);

            if (result.IsOk)
            {
                Action.ButtonType = (ControllerButtonType)Enum.Parse(typeof(ControllerButtonType), result.SelectedItem);
            }
        }

        private async Task SelectSequenceAsync()
        {
            if (Sequences.Any())
            {
                var result = await _dialogService.ShowSelectionDialogAsync(
                    Sequences,
                    Translate("SelectSequence"),
                    Translate("Cancel"),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    Action.SequenceName = result.SelectedItem;
                }
            }
            else
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("NoSequences"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }
        }

        private async Task OpenSequenceEditorAsync()
        {
            var selectedSequence = _creationManager.Sequences.FirstOrDefault(s => s.Name == Action.SequenceName);

            if (selectedSequence != null)
            {
                await NavigationService.NavigateToAsync<SequenceEditorPageViewModel>(new NavigationParameters(("sequence", selectedSequence)));
            }
            else
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("MissingSequence"),
                    Translate("Ok"),
                    _disappearingTokenSource.Token);
            }
        }

        private async Task SelectAxisTypeAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames(typeof(ControllerAxisType)),
                Translate("AxisType"),
                Translate("Cancel"),
                _disappearingTokenSource.Token);

            if (result.IsOk)
            {
                Action.AxisType = (ControllerAxisType)Enum.Parse(typeof(ControllerAxisType), result.SelectedItem);
            }
        }

        private async Task SelectAxisCharacteristicAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames(typeof(ControllerAxisCharacteristic)),
                Translate("AxisCharacteristic"),
                Translate("Cancel"),
                _disappearingTokenSource.Token);

            if (result.IsOk)
            {
                Action.AxisCharacteristic = (ControllerAxisCharacteristic)Enum.Parse(typeof(ControllerAxisCharacteristic), result.SelectedItem);
            }
        }
    }
}
