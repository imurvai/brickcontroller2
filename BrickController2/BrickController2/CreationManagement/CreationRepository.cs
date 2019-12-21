using BrickController2.Database;
using BrickController2.Helpers;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            await _databaseConnection.CreateTableAsync<SequenceControlPoint>();
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
                await _databaseConnection.InsertAsync(controllerProfile);

                if (creation.ControllerProfiles == null)
                {
                    creation.ControllerProfiles = new ObservableCollection<ControllerProfile>();
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
                await _databaseConnection.DeleteAsync(controllerProfile, true);
            }
        }

        public async Task InsertControllerEventAsync(ControllerProfile controllerProfile, ControllerEvent controllerEvent)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(controllerEvent);

                if (controllerProfile.ControllerEvents == null)
                {
                    controllerProfile.ControllerEvents = new ObservableCollection<ControllerEvent>();
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
                await _databaseConnection.InsertAsync(sequence);
            }
        }

        public async Task UpdateSequenceAsync(Sequence sequence)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(sequence);
            }
        }

        public async Task DeleteSequenceAsync(Sequence sequence)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(sequence, true);
            }
        }

        public async Task InsertSequenceControlPointAsync(Sequence sequence, SequenceControlPoint controlPoint)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(controlPoint);

                if (sequence.ControlPoints == null)
                {
                    sequence.ControlPoints = new ObservableCollection<SequenceControlPoint>();
                }

                sequence.ControlPoints.Add(controlPoint);
                await _databaseConnection.UpdateWithChildrenAsync(sequence);
            }
        }

        public async Task UpdateSequenceControlPointAsync(SequenceControlPoint controlPoint)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(controlPoint);
            }
        }

        public async Task DeleteSequenceControlPointAsync(SequenceControlPoint controlPoint)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(controlPoint);
            }
        }
    }
}
