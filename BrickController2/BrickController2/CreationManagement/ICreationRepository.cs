using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public interface ICreationRepository
    {
        Task<List<Creation>> GetCreationsAsync();
        Task InsertCreationAsync(Creation creation);
        Task UodateCreationAsync(Creation creation);
        Task DeleteCreationAsync(Creation creation);

        Task InsertControllerProfileAsync(Creation creation, ControllerProfile controllerProfile);
        Task UpdateControllerProfileAsync(ControllerProfile controllerProfile);
        Task DeleteControllerProfileAsync(ControllerProfile controllerProfile);

        Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent);
        Task UpdateControllerEventAsync(ControllerEvent controllerEvent);
        Task DeleteControllerEventAsync(ControllerEvent controllerEvent);

        Task InsertControllerActionAsync(ControllerEvent controllerEvent, ControllerAction controllerAction);
        Task UpdateControllerActionAsync(ControllerAction controllerAction);
        Task DeleteControllerActionAsync(ControllerAction controllerAction);
    }
}
