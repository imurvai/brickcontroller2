using BrickController2.HardwareServices.GameController;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public interface ICreationManager
    {
        ObservableCollection<Creation> Creations { get; }

        Task LoadCreationsAsync();

        Task<bool> IsCreationNameAvailableAsync(string creationName);
        Task<Creation> AddCreationAsync(string creationName);
        Task DeleteCreationAsync(Creation creation);
        Task RenameCreationAsync(Creation creation, string newName);

        Task<bool> IsControllerProfileNameAvailableAsync(Creation creation, string controllerProfileName);
        Task<ControllerProfile> AddControllerProfileAsync(Creation creation, string controllerProfileName);
        Task DeleteControllerProfileAsync(ControllerProfile controllerProfile);
        Task RenameControllerProfileAsync(ControllerProfile controllerProfile, string newName);

        Task<ControllerEvent> AddOrGetControllerEventAsync(ControllerProfile controllerProfile, GameControllerEventType eventType, string eventCode);
        Task DeleteControllerEventAsync(ControllerEvent controllerEvent);

        Task<ControllerAction> AddOrUpdateControllerActionAsync(ControllerEvent controllerEvent, string deviceId, int channel, bool isInvert, ControllerButtonType buttonType, ControllerAxisCharacteristic axisCharacteristic, int maxOutputPercent, int axisDeadZonePercent);
        Task DeleteControllerActionAsync(ControllerAction controllerAction);
        Task UpdateControllerActionAsync(ControllerAction controllerAction, string deviceId, int channel, bool isInvert, ControllerButtonType buttonType, ControllerAxisCharacteristic axisCharacteristic, int maxOutputPercent, int axisDeadZonePercent);
    }
}
