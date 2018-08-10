using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BrickController2.CreationManagement
{
    public class CreationRepository : ICreationRepository
    {
        private const string CreationDatabaseName = "creations.db3";
        private readonly SQLiteAsyncConnection _databaseConnection;
        private readonly AsyncLock _lock = new AsyncLock();

        public CreationRepository()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), CreationDatabaseName);

            _databaseConnection = new SQLiteAsyncConnection(databasePath);
            _databaseConnection.CreateTableAsync<Creation>().Wait();
            _databaseConnection.CreateTableAsync<ControllerProfile>().Wait();
            _databaseConnection.CreateTableAsync<ControllerEvent>().Wait();
            _databaseConnection.CreateTableAsync<ControllerAction>().Wait();
        }

        public async Task<List<Creation>> GetCreationsAsync()
        {
            using (_lock.LockAsync())
            {
                return await _databaseConnection.GetAllWithChildrenAsync<Creation>();
            }
        }

        public async Task InsertCreationAsync(Creation creation)
        {
            using (_lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(creation);
            }
        }

        public async Task UodateCreationAsync(Creation creation)
        {
            using (_lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(creation);
            }
        }

        public async Task DeleteCreationAsync(Creation creation)
        {
            using (_lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(creation);
            }
        }

        public async Task InsertControllerProfileAsync(Creation creation, ControllerProfile controllerProfile)
        {
            using (_lock.LockAsync())
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
            using (_lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(controllerProfile);
            }
        }

        public async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            using (_lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(controllerProfile);
            }
        }

        public async Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent)
        {
            using (_lock.LockAsync())
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
            using (_lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(controllerEvent);
            }
        }

        public async Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            using (_lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(controllerEvent);
            }
        }

        public async Task InsertControllerActionAsync(ControllerEvent controllerEvent, ControllerAction controllerAction)
        {
            using (_lock.LockAsync())
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

        public Task UpdateControllerActionAsync(ControllerAction controllerAction)
        {
            using (_lock.LockAsync())
            {
                return _databaseConnection.UpdateAsync(controllerAction);
            }
        }

        public Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            using (_lock.LockAsync())
            {
                return _databaseConnection.DeleteAsync(controllerAction);
            }
        }
    }
}
