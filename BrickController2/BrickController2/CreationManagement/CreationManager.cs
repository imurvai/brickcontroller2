using BrickController2.PlatformServices.GameController;
using BrickController2.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public class CreationManager : ICreationManager
    {
        private readonly ICreationRepository _creationRepository;
        private readonly AsyncLock _asyncLock = new AsyncLock();

        public CreationManager(ICreationRepository creationRepository)
        {
            _creationRepository = creationRepository;
        }

        public ObservableCollection<Creation> Creations { get; } = new ObservableCollection<Creation>();

        public async Task LoadCreationsAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                Creations.Clear();

                var creations = await _creationRepository.GetCreationsAsync();
                foreach (var creation in creations)
                {
                    Creations.Add(creation);
                }
            }
        }

        public async Task<bool> IsCreationNameAvailableAsync(string creationName)
        {
            using (await _asyncLock.LockAsync())
            {
                return Creations.All(c => c.Name != creationName);
            }
        }

        public async Task<Creation> AddCreationAsync(string creationName)
        {
            using (await _asyncLock.LockAsync())
            {
                var creation = new Creation { Name = creationName };
                await _creationRepository.InsertCreationAsync(creation);
                Creations.Add(creation);
                return creation;
            }
        }

        public async Task DeleteCreationAsync(Creation creation)
        {
            using (await _asyncLock.LockAsync())
            {
                await _creationRepository.DeleteCreationAsync(creation);
                Creations.Remove(creation);
            }
        }

        public async Task RenameCreationAsync(Creation creation, string newName)
        {
            using (await _asyncLock.LockAsync())
            {
                creation.Name = newName;
                await _creationRepository.UpdateCreationAsync(creation);
            }
        }

        public async Task<bool> IsControllerProfileNameAvailableAsync(Creation creation, string controllerProfileName)
        {
            using (await _asyncLock.LockAsync())
            {
                return creation.ControllerProfiles.All(cp => cp.Name != controllerProfileName);
            }
        }

        public async Task<ControllerProfile> AddControllerProfileAsync(Creation creation, string controllerProfileName)
        {
            using (await _asyncLock.LockAsync())
            {
                var controllerProfile = new ControllerProfile { Name = controllerProfileName };
                await _creationRepository.InsertControllerProfileAsync(creation, controllerProfile);
                return controllerProfile;
            }
        }

        public async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            using (await _asyncLock.LockAsync())
            {
                var parent = controllerProfile.Creation;
                await _creationRepository.DeleteControllerProfileAsync(controllerProfile);
                parent.ControllerProfiles.Remove(controllerProfile);
            }
        }

        public async Task RenameControllerProfileAsync(ControllerProfile controllerProfile, string newName)
        {
            using (await _asyncLock.LockAsync())
            {
                controllerProfile.Name = newName;
                await _creationRepository.UpdateControllerProfileAsync(controllerProfile);
            }
        }

        public async Task<ControllerEvent> AddOrGetControllerEventAsync(ControllerProfile controllerProfile, GameControllerEventType eventType, string eventCode)
        {
            using (await _asyncLock.LockAsync())
            {
                var controllerEvent = controllerProfile.ControllerEvents.FirstOrDefault(ce => ce.EventType == eventType && ce.EventCode == eventCode);
                if (controllerEvent == null)
                {
                    controllerEvent = new ControllerEvent { EventType = eventType, EventCode = eventCode };
                    await _creationRepository.InsertControllerEventAsync(controllerProfile, controllerEvent);
                }

                return controllerEvent;
            }
        }

        public async Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            using (await _asyncLock.LockAsync())
            {
                var parent = controllerEvent.ControllerProfile;
                await _creationRepository.DeleteControllerEventAsync(controllerEvent);
                parent.ControllerEvents.Remove(controllerEvent);
            }
        }

        public async Task<ControllerAction> AddOrUpdateControllerActionAsync(
            ControllerEvent controllerEvent,
            string deviceId,
            int channel,
            bool isInvert,
            ControllerButtonType buttonType,
            ControllerAxisType axisType,
            ControllerAxisCharacteristic axisCharacteristic,
            int maxOutputPercent,
            int axisDeadZonePercent,
            ChannelOutputType channelOutputType,
            int maxServoAngle,
            int servoBaseAngle,
            int stepperAngle)
        {
            using (await _asyncLock.LockAsync())
            {
                var controllerAction = controllerEvent.ControllerActions.FirstOrDefault(ca => ca.DeviceId == deviceId && ca.Channel == channel);
                if (controllerAction != null)
                {
                    controllerAction.IsInvert = isInvert;
                    controllerAction.ButtonType = buttonType;
                    controllerAction.AxisType = axisType;
                    controllerAction.AxisCharacteristic = axisCharacteristic;
                    controllerAction.MaxOutputPercent = maxOutputPercent;
                    controllerAction.AxisDeadZonePercent = axisDeadZonePercent;
                    controllerAction.ChannelOutputType = channelOutputType;
                    controllerAction.MaxServoAngle = maxServoAngle;
                    controllerAction.ServoBaseAngle = servoBaseAngle;
                    controllerAction.StepperAngle = stepperAngle;
                    await _creationRepository.UpdateControllerActionAsync(controllerAction);
                }
                else
                {
                    controllerAction = new ControllerAction
                    {
                        DeviceId = deviceId,
                        Channel = channel,
                        IsInvert = isInvert,
                        ButtonType = buttonType,
                        AxisType = axisType,
                        AxisCharacteristic = axisCharacteristic,
                        MaxOutputPercent = maxOutputPercent,
                        AxisDeadZonePercent = axisDeadZonePercent,
                        ChannelOutputType = channelOutputType,
                        MaxServoAngle = maxServoAngle,
                        ServoBaseAngle = servoBaseAngle,
                        StepperAngle = stepperAngle
                    };
                    await _creationRepository.InsertControllerActionAsync(controllerEvent, controllerAction);
                }

                return controllerAction;
            }
        }

        public async Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            using (await _asyncLock.LockAsync())
            {
                var parent = controllerAction.ControllerEvent;
                await _creationRepository.DeleteControllerActionAsync(controllerAction);
                parent.ControllerActions.Remove(controllerAction);
            }
        }

        public async Task UpdateControllerActionAsync(
            ControllerAction controllerAction,
            string deviceId,
            int channel,
            bool isInvert,
            ControllerButtonType buttonType,
            ControllerAxisType axisType,
            ControllerAxisCharacteristic axisCharacteristic,
            int maxOutputPercent,
            int axisDeadZonePercent,
            ChannelOutputType channelOutputType,
            int maxServoAngle,
            int servoBaseAngle,
            int stepperAngle)
        {
            using (await _asyncLock.LockAsync())
            {
                var otherControllerAction = controllerAction.ControllerEvent.ControllerActions.FirstOrDefault(ca => ca.Id != controllerAction.Id && ca.DeviceId == deviceId && ca.Channel == channel);
                if (otherControllerAction != null)
                {
                    var parent = otherControllerAction.ControllerEvent;
                    await _creationRepository.DeleteControllerActionAsync(otherControllerAction);
                    parent.ControllerActions.Remove(otherControllerAction);
                }

                controllerAction.DeviceId = deviceId;
                controllerAction.Channel = channel;
                controllerAction.IsInvert = isInvert;
                controllerAction.ButtonType = buttonType;
                controllerAction.AxisType = axisType;
                controllerAction.AxisCharacteristic = axisCharacteristic;
                controllerAction.MaxOutputPercent = maxOutputPercent;
                controllerAction.AxisDeadZonePercent = axisDeadZonePercent;
                controllerAction.ChannelOutputType = channelOutputType;
                controllerAction.MaxServoAngle = maxServoAngle;
                controllerAction.ServoBaseAngle = servoBaseAngle;
                controllerAction.StepperAngle = stepperAngle;
                await _creationRepository.UpdateControllerActionAsync(controllerAction);
            }
        }
    }
}
