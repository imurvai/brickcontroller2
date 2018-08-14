using BrickController2.Database;
using BrickController2.Helpers;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public class DeviceRepository : IDeviceRepository
    {
        private const string CreationDatabaseName = "devices.db3";
        private readonly SQLiteAsyncConnection _databaseConnection;
        private readonly AsyncLock _lock = new AsyncLock();
        private bool _inited;

        public DeviceRepository(SQLiteAsyncConnectionFactory connectionFactory)
        {
            _databaseConnection = connectionFactory(CreationDatabaseName);
        }

        private async Task InitAsync()
        {
            if (_inited)
            {
                return;
            }

            await _databaseConnection.CreateTableAsync<DeviceDTO>();
            _inited = true;
        }

        public async Task<IEnumerable<DeviceDTO>> GetDevices()
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                return await _databaseConnection.Table<DeviceDTO>().ToListAsync();
            }
        }

        public async Task InsertDevice(DeviceDTO device)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.InsertAsync(device);
            }
        }

        public async Task DeleteDevice(DeviceDTO device)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(device);
            }
        }

        public async Task UpdateDevice(DeviceDTO device)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(device);
            }
        }
    }
}
