using BrickController2.PlatformServices.GameController;
using System.Collections.ObjectModel;

namespace BrickController2.CreationManagement
{
    public interface ICreationManager
    {
        ObservableCollection<Creation> Creations { get; }
        ObservableCollection<Sequence> Sequences { get; }

        Task LoadCreationsAndSequencesAsync();
        Task ImportCreationAsync(Creation creation);
        Task ImportCreationAsync(string creationFilename);
        Task ImportCreationPayloadAsync(string creationJson);
        Task ExportCreationAsync(Creation creation, string creationFilename);
        Task<string> ExportCreationAsync(Creation creation);
        Task<bool> IsCreationNameAvailableAsync(string creationName);
        Task<Creation> AddCreationAsync(string creationName);
        Task DeleteCreationAsync(Creation creation);
        Task RenameCreationAsync(Creation creation, string newName);

        Task ImportControllerProfileAsync(Creation creation, string controllerProfileFilename);
        Task ExportControllerProfileAsync(ControllerProfile controllerProfile, string ControllerProfileFilename);
        Task<bool> IsControllerProfileNameAvailableAsync(Creation creation, string controllerProfileName);
        Task<ControllerProfile> AddControllerProfileAsync(Creation creation, string controllerProfileName);
        Task DeleteControllerProfileAsync(ControllerProfile controllerProfile);
        Task RenameControllerProfileAsync(ControllerProfile controllerProfile, string newName);

        Task<ControllerEvent> AddOrGetControllerEventAsync(ControllerProfile controllerProfile, GameControllerEventType eventType, string eventCode);
        Task DeleteControllerEventAsync(ControllerEvent controllerEvent);

        Task<ControllerAction> AddOrUpdateControllerActionAsync(
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
            string sequenceName);
        Task DeleteControllerActionAsync(ControllerAction controllerAction);
        Task UpdateControllerActionAsync(
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
            string sequenceName);

        Task ImportSequenceAsync(string sequenceFilename);
        Task ExportSequenceAsync(Sequence sequence, string sequenceFilename);
        Task<bool> IsSequenceNameAvailableAsync(string sequenceName);
        Task<Sequence> AddSequenceAsync(string sequenceName);
        Task UpdateSequenceAsync(Sequence sequence, string sequenceName, bool loop, bool interpolate, IEnumerable<SequenceControlPoint> controlPoints);
        Task DeleteSequenceAsync(Sequence sequence);
    }
}
