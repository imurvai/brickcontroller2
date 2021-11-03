using BrickController2.Database;
using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public class CreationRepository : ICreationRepository
    {
        private const string CreationDatabaseName = "creations.db3";
        private readonly SQLiteAsyncConnection _databaseConnection;
        private readonly AsyncLock _lock = new AsyncLock();
        private bool _inited;

        public CreationRepository(SQLiteAsyncConnectionFactory connectionFactory)
        {
            _databaseConnection = connectionFactory(CreationDatabaseName);
        }

        private async Task InitAsync()
        {
            if (_inited)
            {
                return;
            }

            await _databaseConnection.CreateTableAsync<Creation>();
            await _databaseConnection.CreateTableAsync<ControllerProfile>();
            await _databaseConnection.CreateTableAsync<ControllerEvent>();
            await _databaseConnection.CreateTableAsync<ControllerAction>();
            await _databaseConnection.CreateTableAsync<Sequence>();
            _inited = true;
        }

        public async Task<List<Creation>> GetCreationsAsync()
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                return await _databaseConnection.GetAllWithChildrenAsync<Creation>(null, true);
            }
        }

        public async Task<List<Sequence>> GetSequencesAsync()
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                return await _databaseConnection.GetAllWithChildrenAsync<Sequence>(null, true);
            }
        }

        public async Task InsertCreationAsync(Creation creation)
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                await _databaseConnection.InsertAsync(creation);

                // insert all available profiles as well as it's present for a creation when it's imported only
                if (creation.ControllerProfiles.Any())
                {
                    foreach (var profile in creation.ControllerProfiles)
                    {
                        await InsertControllerProfileAsync(profile);
                    }
                    await _databaseConnection.UpdateWithChildrenAsync(creation);
                }
            }
        }

        public async Task UpdateCreationAsync(Creation creation)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(creation);
            }
        }

        public async Task DeleteCreationAsync(Creation creation)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(creation, true);
            }
        }

        public async Task InsertControllerProfileAsync(Creation creation, ControllerProfile controllerProfile)
        {
            using (await _lock.LockAsync())
            {
                await InsertControllerProfileAsync(controllerProfile);

                if (creation.ControllerProfiles == null)
                {
                    creation.ControllerProfiles = new ObservableCollection<ControllerProfile>();
                }

                creation.ControllerProfiles.Add(controllerProfile);
                await _databaseConnection.UpdateWithChildrenAsync(creation);
            }
        }

        private async Task InsertControllerProfileAsync(ControllerProfile controllerProfile)
        {
            await _databaseConnection.InsertAsync(controllerProfile);

            // insert all available events as well as it's present for a profile when it's imported only
            if (controllerProfile.ControllerEvents.Any())
            {
                foreach (var controllerEvent in controllerProfile.ControllerEvents)
                {
                    await InsertControllerEventAsync(controllerEvent);
                }
                await _databaseConnection.UpdateWithChildrenAsync(controllerProfile);
            }
        }

        public async Task UpdateControllerProfileAsync(ControllerProfile controllerProfile)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(controllerProfile);
            }
        }

        public async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(controllerProfile, true);
            }
        }

        public async Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent)
        {
            using (await _lock.LockAsync())
            {
                await InsertControllerEventAsync(controllerEvent);

                if (controllerProfile.ControllerEvents == null)
                {
                    controllerProfile.ControllerEvents = new ObservableCollection<ControllerEvent>();
                }

                controllerProfile.ControllerEvents.Add(controllerEvent);
                await _databaseConnection.UpdateWithChildrenAsync(controllerProfile);
            }
        }

        private async Task InsertControllerEventAsync(ControllerEvent controllerEvent)
        {
            await _databaseConnection.InsertAsync(controllerEvent);

            // insert all available action as well as it's present for an event when it's imported only
            if (controllerEvent.ControllerActions.Any())
            {
                foreach (var controllerAction in controllerEvent.ControllerActions)
                {
                    await _databaseConnection.InsertAsync(controllerAction);
                }
                await _databaseConnection.UpdateWithChildrenAsync(controllerEvent);
            }
        }

        public async Task UpdateControllerEventAsync(ControllerEvent controllerEvent)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(controllerEvent);
            }
        }

        public async Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(controllerEvent, true);
            }
        }

        public async Task InsertControllerActionAsync(ControllerEvent controllerEvent, ControllerAction controllerAction)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(controllerAction);

                if (controllerEvent.ControllerActions == null)
                {
                    controllerEvent.ControllerActions = new ObservableCollection<ControllerAction>();
                }

                controllerEvent.ControllerActions.Add(controllerAction);
                await _databaseConnection.UpdateWithChildrenAsync(controllerEvent);
            }
        }

        public async Task UpdateControllerActionAsync(ControllerAction controllerAction)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(controllerAction);
            }
        }

        public async Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(controllerAction);
            }
        }

        public async Task InsertSequenceAsync(Sequence sequence)
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                await _databaseConnection.InsertWithChildrenAsync(sequence);
            }
        }

        public async Task UpdateSequenceAsync(Sequence sequence)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateWithChildrenAsync(sequence);
            }
        }

        public async Task DeleteSequenceAsync(Sequence sequence)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(sequence, true);
            }
        }
    }
}
