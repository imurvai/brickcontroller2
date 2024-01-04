using BrickController2.PlatformServices.GameController;
using BrickController2.Helpers;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

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
        public ObservableCollection<Sequence> Sequences { get; } = new ObservableCollection<Sequence>();

        public async Task LoadCreationsAndSequencesAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                Creations.Clear();

                var creations = await _creationRepository.GetCreationsAsync();
                foreach (var creation in creations)
                {
                    Creations.Add(creation);
                }

                Sequences.Clear();

                var sequences = await _creationRepository.GetSequencesAsync();
                foreach (var sequence in sequences)
                {
                    Sequences.Add(sequence);
                }
            }
        }

        public async Task ImportCreationAsync(string creationFilename)
        {
            using (await _asyncLock.LockAsync())
            {
                var creationJson = await File.ReadAllTextAsync(creationFilename);
                var creation = JsonConvert.DeserializeObject<Creation>(creationJson);

                var creationName = creation.Name;
                if (!IsCreationNameAvailable(creationName))
                {
                    for (var suffix = 1; suffix < 1000; suffix++)
                    {
                        var newCreationName = $"{creationName} {suffix}";
                        if (IsCreationNameAvailable(newCreationName))
                        {
                            creationName = newCreationName;
                            break;
                        }
                    }
                }

                creation.Name = creationName;
                await _creationRepository.InsertCreationAsync(creation);
                Creations.Add(creation);
            }
        }

        public async Task ExportCreationAsync(Creation creation, string creationFilename)
        {
            using (await _asyncLock.LockAsync())
            {
                var creationJson = JsonConvert.SerializeObject(creation);
                await File.WriteAllTextAsync(creationFilename, creationJson);
            }
        }

        public async Task<bool> IsCreationNameAvailableAsync(string creationName)
        {
            using (await _asyncLock.LockAsync())
            {
                return IsCreationNameAvailable(creationName);
            }
        }

        private bool IsCreationNameAvailable(string creationName)
        {
            return Creations.All(c => c.Name != creationName);
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
                return IsControllerProfileNameAvailable(creation, controllerProfileName);
            }
        }

        private bool IsControllerProfileNameAvailable(Creation creation, string controllerProfileName)
        {
            return creation.ControllerProfiles.All(cp => cp.Name != controllerProfileName);
        }

        public async Task ImportControllerProfileAsync(Creation creation, string controllerProfileFilename)
        {
            using (await _asyncLock.LockAsync())
            {
                var controllerProfileJson = await File.ReadAllTextAsync(controllerProfileFilename);
                var controllerProfile = JsonConvert.DeserializeObject<ControllerProfile>(controllerProfileJson);

                controllerProfile.Creation = null;
                controllerProfile.CreationId = 0;

                var controllerProfileName = controllerProfile.Name;
                if (!IsControllerProfileNameAvailable(creation, controllerProfileName))
                {
                    for (var suffix = 1; suffix < 1000; suffix++)
                    {
                        var newControllerProfileName = $"{controllerProfileName} {suffix}";
                        if (IsControllerProfileNameAvailable(creation, newControllerProfileName))
                        {
                            controllerProfileName = newControllerProfileName;
                            break;
                        }
                    }
                }

                controllerProfile.Name = controllerProfileName;
                await _creationRepository.InsertControllerProfileAsync(creation, controllerProfile);
            }
        }

        public async Task ExportControllerProfileAsync(ControllerProfile controllerProfile, string controllerProfileFilename)
        {
            using (await _asyncLock.LockAsync())
            {
                var controllerProfileJson = JsonConvert.SerializeObject(controllerProfile);
                await File.WriteAllTextAsync(controllerProfileFilename, controllerProfileJson);
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
            int axisActiveZonePercent,
            int axisDeadZonePercent,
            ChannelOutputType channelOutputType,
            int maxServoAngle,
            int servoBaseAngle,
            int stepperAngle,
            string sequenceName)
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
                    controllerAction.AxisActiveZonePercent = axisActiveZonePercent;
                    controllerAction.AxisDeadZonePercent = axisDeadZonePercent;
                    controllerAction.ChannelOutputType = channelOutputType;
                    controllerAction.MaxServoAngle = maxServoAngle;
                    controllerAction.ServoBaseAngle = servoBaseAngle;
                    controllerAction.StepperAngle = stepperAngle;
                    controllerAction.SequenceName = sequenceName;
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
                        AxisActiveZonePercent = axisActiveZonePercent,
                        AxisDeadZonePercent = axisDeadZonePercent,
                        ChannelOutputType = channelOutputType,
                        MaxServoAngle = maxServoAngle,
                        ServoBaseAngle = servoBaseAngle,
                        StepperAngle = stepperAngle,
                        SequenceName = sequenceName
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
            int axisActiveZonePercent,
            int axisDeadZonePercent,
            ChannelOutputType channelOutputType,
            int maxServoAngle,
            int servoBaseAngle,
            int stepperAngle,
            string sequenceName)
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
                controllerAction.AxisActiveZonePercent = axisActiveZonePercent;
                controllerAction.AxisDeadZonePercent = axisDeadZonePercent;
                controllerAction.ChannelOutputType = channelOutputType;
                controllerAction.MaxServoAngle = maxServoAngle;
                controllerAction.ServoBaseAngle = servoBaseAngle;
                controllerAction.StepperAngle = stepperAngle;
                controllerAction.SequenceName = sequenceName;
                await _creationRepository.UpdateControllerActionAsync(controllerAction);
            }
        }

        public async Task ImportSequenceAsync(string sequenceFilename)
        {
            using (await _asyncLock.LockAsync())
            {
                var sequenceJson = await File.ReadAllTextAsync(sequenceFilename);
                var sequence = JsonConvert.DeserializeObject<Sequence>(sequenceJson);

                sequence.Id = 0;

                var sequenceName = sequence.Name;
                if (!IsSequenceNameAvailable(sequenceName))
                {
                    for (var suffix = 1; suffix < 1000; suffix++)
                    {
                        var newSequenceName = $"{sequenceName} {suffix}";
                        if (IsSequenceNameAvailable(newSequenceName))
                        {
                            sequenceName = newSequenceName;
                            break;
                        }
                    }
                }

                sequence.Name = sequenceName;
                await _creationRepository.InsertSequenceAsync(sequence);
                Sequences.Add(sequence);
            }
        }

        public async Task ExportSequenceAsync(Sequence sequence, string sequenceFilename)
        {
            using (await _asyncLock.LockAsync())
            {
                var sequenceJson = JsonConvert.SerializeObject(sequence);
                await File.WriteAllTextAsync(sequenceFilename, sequenceJson);
            }
        }

        public async Task<bool> IsSequenceNameAvailableAsync(string sequenceName)
        {
            using (await _asyncLock.LockAsync())
            {
                return IsSequenceNameAvailable(sequenceName);
            }
        }

        private bool IsSequenceNameAvailable(string sequenceName)
        {
            return Sequences.All(s => s.Name != sequenceName);
        }

        public async Task<Sequence> AddSequenceAsync(string sequenceName)
        {
            using (await _asyncLock.LockAsync())
            {
                var sequence = new Sequence { Name = sequenceName };
                await _creationRepository.InsertSequenceAsync(sequence);

                Sequences.Add(sequence);
                return sequence;
            }
        }

        public async Task UpdateSequenceAsync(Sequence sequence, string sequenceName, bool loop, bool interpolate, IEnumerable<SequenceControlPoint> controlPoints)
        {
            var sequenceOriginalName = sequence.Name;

            using (await _asyncLock.LockAsync())
            {
                sequence.Name = sequenceName;
                sequence.Loop = loop;
                sequence.Interpolate = interpolate;
                sequence.ControlPoints = new ObservableCollection<SequenceControlPoint>(controlPoints);
                await _creationRepository.UpdateSequenceAsync(sequence);
            }

            if (sequenceOriginalName != sequenceName)
            {
                foreach (var creation in Creations)
                {
                    foreach (var controllerProfile in creation.ControllerProfiles)
                    {
                        foreach (var controllerEvent in controllerProfile.ControllerEvents)
                        {
                            foreach (var controllerAction in controllerEvent.ControllerActions)
                            {
                                if (controllerAction.SequenceName == sequenceOriginalName)
                                {
                                    await UpdateControllerActionAsync(
                                        controllerAction,
                                        controllerAction.DeviceId,
                                        controllerAction.Channel,
                                        controllerAction.IsInvert,
                                        controllerAction.ButtonType,
                                        controllerAction.AxisType,
                                        controllerAction.AxisCharacteristic,
                                        controllerAction.MaxOutputPercent,
                                        controllerAction.AxisActiveZonePercent,
                                        controllerAction.AxisDeadZonePercent,
                                        controllerAction.ChannelOutputType,
                                        controllerAction.MaxServoAngle,
                                        controllerAction.ServoBaseAngle,
                                        controllerAction.StepperAngle,
                                        sequenceName);
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task DeleteSequenceAsync(Sequence sequence)
        {
            using (await _asyncLock.LockAsync())
            {
                await _creationRepository.DeleteSequenceAsync(sequence);
                Sequences.Remove(sequence);
            }
        }
    }
}
