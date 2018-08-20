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

        public async Task<IEnumerable<DeviceDTO>> GetDevicesAsync()
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                return await _databaseConnection.Table<DeviceDTO>().ToListAsync();
            }
        }

        public async Task DeleteDevicesAsync()
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                await _databaseConnection.ExecuteAsync("DELETE FROM Device");
            }
        }

        public async Task InsertDeviceAsync(DeviceDTO device)
        {
            using (await _lock.LockAsync())
            {
                await InitAsync();
                await _databaseConnection.InsertAsync(device);
            }
        }

        public async Task DeleteDeviceAsync(DeviceDTO device)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.DeleteAsync(device);
            }
        }

        public async Task UpdateDeviceAsync(DeviceDTO device)
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.UpdateAsync(device);
            }
        }
    }
}
