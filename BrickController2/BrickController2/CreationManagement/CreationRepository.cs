using BrickController2.Database;
using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public class CreationRepository : ICreationRepository
    {
        private const string CreationDatabaseName = "creations.db3";
        private readonly SQLiteAsyncConnection _databaseConnection;
        private readonly AsyncLock _lock = new AsyncLock();
        private bool _inited;

        public CreationRepository(SQLiteAsyncConnectionFactory connectionFaRRctory)
        {
            _databaseConnection = connectionFaRRctory(CreationDatabaseName);
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
            _inited = true;
        }

        public async Task<List<Creation>> GetCreationsAsync()
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                return await _databaseConnection.GetAllWithChildrenAsync<Creation>();
            }
        }

        public async Task InsertCreationAsync(Creation creation)
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                await _databaseConnection.InsertAsync(creation);
            }
        }

        public async Task UodateCreationAsync(Creation creation)
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
                await _databaseConnection.DeleteAsync(creation);
            }
        }

        public async Task InsertControllerProfileAsync(Creation creation, ControllerProfile controllerProfile)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(controllerProfile);

                if (creation.ControllerProfiles == null)
                {
                    creation.ControllerProfiles = new List<ControllerProfile>();
                }

                creation.ControllerProfiles.Add(controllerProfile);
                await _databaseConnection.UpdateWithChildrenAsync(creation);
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
                await _databaseConnection.DeleteAsync(controllerProfile);
            }
        }

        public async Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(controllerEvent);

                if (controllerProfile.ControllerEvents == null)
                {
                    controllerProfile.ControllerEvents = new List<ControllerEvent>();
                }

                controllerProfile.ControllerEvents.Add(controllerEvent);
                await _databaseConnection.UpdateWithChildrenAsync(controllerProfile);
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
                await _databaseConnection.DeleteAsync(controllerEvent);
            }
        }

        public async Task InsertControllerActionAsync(ControllerEvent controllerEvent, ControllerAction controllerAction)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(controllerAction);

                if (controllerEvent.ControllerActions == null)
                {
                    controllerEvent.ControllerActions = new List<ControllerAction>();
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
    }
}
