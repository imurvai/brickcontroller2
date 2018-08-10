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

        public CreationRepository()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), CreationDatabaseName);

            _databaseConnection = new SQLiteAsyncConnection(databasePath);
            _databaseConnection.CreateTableAsync<Creation>().Wait();
            _databaseConnection.CreateTableAsync<ControllerProfile>().Wait();
            _databaseConnection.CreateTableAsync<ControllerEvent>().Wait();
            _databaseConnection.CreateTableAsync<ControllerAction>().Wait();
        }

        public Task<List<Creation>> GetCreationsAsync()
        {
            return _databaseConnection.GetAllWithChildrenAsync<Creation>();
        }

        public Task InsertCreationAsync(Creation creation)
        {
            return _databaseConnection.InsertAsync(creation);
        }

        public Task UodateCreationAsync(Creation creation)
        {
            return _databaseConnection.UpdateAsync(creation);
        }

        public Task DeleteCreationAsync(Creation creation)
        {
            return _databaseConnection.DeleteAsync(creation);
        }

        public async Task InsertControllerProfileAsync(Creation creation, ControllerProfile controllerProfile)
        {
            await _databaseConnection.InsertAsync(controllerProfile);

            if (creation.ControllerProfiles == null)
            {
                creation.ControllerProfiles = new List<ControllerProfile>();
            }

            creation.ControllerProfiles.Add(controllerProfile);
            await _databaseConnection.UpdateWithChildrenAsync(creation);
        }

        public Task UpdateControllerProfileAsync(ControllerProfile controllerProfile)
        {
            return _databaseConnection.UpdateAsync(controllerProfile);
        }

        public Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            return _databaseConnection.DeleteAsync(controllerProfile);
        }

        public async Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent)
        {
            await _databaseConnection.InsertAsync(controllerEvent);

            if (controllerProfile.ControllerEvents == null)
            {
                controllerProfile.ControllerEvents = new List<ControllerEvent>();
            }

            controllerProfile.ControllerEvents.Add(controllerEvent);
            await _databaseConnection.UpdateWithChildrenAsync(controllerProfile);
        }

        public Task UpdateControllerEventAsync(ControllerEvent controllerEvent)
        {
            return _databaseConnection.UpdateAsync(controllerEvent);
        }

        public Task DeleteControllerEventAsync(ControllerEvent controllerEvent)
        {
            return _databaseConnection.DeleteAsync(controllerEvent);
        }

        public async Task InsertControllerActionAsync(ControllerEvent controllerEvent, ControllerAction controllerAction)
        {
            await _databaseConnection.InsertAsync(controllerAction);

            if (controllerEvent.ControllerActions == null)
            {
                controllerEvent.ControllerActions = new List<ControllerAction>();
            }

            controllerEvent.ControllerActions.Add(controllerAction);
            await _databaseConnection.UpdateWithChildrenAsync(controllerEvent);
        }

        public Task UpdateControllerActionAsync(ControllerAction controllerAction)
        {
            return _databaseConnection.UpdateAsync(controllerAction);
        }

        public Task DeleteControllerActionAsync(ControllerAction controllerAction)
        {
            return _databaseConnection.DeleteAsync(controllerAction);
        }
    }
}
