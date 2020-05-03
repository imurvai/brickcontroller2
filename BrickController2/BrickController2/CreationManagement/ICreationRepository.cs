using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public interface ICreationRepository
    {
        Task<List<Creation>> GetCreationsAsync();
        Task<List<Sequence>> GetSequencesAsync();

        Task InsertCreationAsync(Creation creation);
        Task UpdateCreationAsync(Creation creation);
        Task DeleteCreationAsync(Creation creation);

        Task InsertControllerProfileAsync(Creation creation, ControllerProfile controllerProfile);
        Task UpdateControllerProfileAsync(ControllerProfile controllerProfile);
        Task DeleteControllerProfileAsync(ControllerProfile controllerProfile);
        Task<ControllerProfile> CopyControllerProfileAsync(Creation creation, ControllerProfile sourceProfile, string copiedProfileName);

        Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent);
        Task UpdateControllerEventAsync(ControllerEvent controllerEvent);
        Task DeleteControllerEventAsync(ControllerEvent controllerEvent);

        Task InsertControllerActionAsync(ControllerEvent controllerEvent, ControllerAction controllerAction);
        Task UpdateControllerActionAsync(ControllerAction controllerAction);
        Task DeleteControllerActionAsync(ControllerAction controllerAction);

        Task InsertSequenceAsync(Sequence sequence);
        Task UpdateSequenceAsync(Sequence sequence);
        Task DeleteSequenceAsync(Sequence sequence);
    }
}
