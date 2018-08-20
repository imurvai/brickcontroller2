using BrickController2.Helpers;
using System;
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
                if (!(await IsCreationNameAvailableAsync(creationName)))
                {
                    throw new ArgumentException($"Creation with same name already exists ({creationName}).");
                }

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
                if (!(await IsCreationNameAvailableAsync(newName)))
                {
                    throw new ArgumentException($"Creation already exists with name {newName}.");
                }

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
                if (!(await IsControllerProfileNameAvailableAsync(creation, controllerProfileName)))
                {
                    throw new ArgumentException($"Controller profile already exists with name {controllerProfileName}.");
                }

                var controllerProfile = new ControllerProfile { Name = controllerProfileName };
                await _creationRepository.InsertControllerProfileAsync(creation, controllerProfile);
                return controllerProfile;
            }
        }

        public async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            using (await _asyncLock.LockAsync())
            {
                await _creationRepository.DeleteControllerProfileAsync(controllerProfile);
            }
        }

        public async Task RenameControllerProfileAsync(ControllerProfile controllerProfile, string newName)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!(await IsControllerProfileNameAvailableAsync(controllerProfile.Creation, newName)))
                {
                    throw new ArgumentException($"Controller profile already exists with name {newName}.");
                }

                controllerProfile.Name = newName;
                await _creationRepository.UpdateControllerProfileAsync(controllerProfile);
            }
        }

        public async Task<ControllerEvent> AddOrGetControllerEventAsync(ControllerProfile controllerProfile, ControllerEventType eventType, string eventCode)
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
                await _creationRepository.DeleteControllerEventAsync(controllerEvent);
            }
        }

        public async Task<ControllerAction> AddOrUpdateControllerActionAsync(ControllerEvent controllerEvent, string deviceId, int channel, bool isInvert, bool isToggle, int maxOutput)
        {
            using (await _asyncLock.LockAsync())
            {
                var controllerAction = controllerEvent.ControllerActions.FirstOrDefault(ca => ca.DeviceID == deviceId && ca.Channel == channel);
                if (controllerAction != null)
                {
                    controllerAction.IsInvert = isInvert;
                    controllerAction.IsToggle = isToggle;
                    controllerAction.MaxOutput = maxOutput;
                    await _creationRepository.UpdateControllerActionAsync(controllerAction);
                }
                else
                {
                    controllerAction = new ControllerAction { DeviceID = deviceId, Channel = channel, IsInvert = isInvert, IsToggle = isToggle, MaxOutput = maxOutput };
                    await _creationRepository.InsertControllerActionAsync(controllerEvent, controllerAction);
                }

                return controllerAction;
            }
        }

        public async Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            using (await _asyncLock.LockAsync())
            {
                await _creationRepository.DeleteControllerActionAsync(controllerAction);
            }
        }

        public async Task UpdateControllerActionAsync(ControllerAction controllerAction, string deviceId, int channel, bool isInvert, bool isToggle, int maxOutput)
        {
            using (await _asyncLock.LockAsync())
            {
                var otherControllerAction = controllerAction.ControllerEvent.ControllerActions.FirstOrDefault(ca => ca.DeviceID == deviceId && ca.Channel == channel);
                if (otherControllerAction != null)
                {
                    await _creationRepository.DeleteControllerActionAsync(otherControllerAction);
                }

                controllerAction.DeviceID = deviceId;
                controllerAction.Channel = channel;
                controllerAction.IsInvert = isInvert;
                controllerAction.IsToggle = isToggle;
                controllerAction.MaxOutput = maxOutput;
                await _creationRepository.UpdateControllerActionAsync(controllerAction);
            }
        }
    }
}
