using BrickController2.Database;
using BrickController2.Helpers;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal class DeviceRepository : IDeviceRepository
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

        public async Task InsertDeviceAsync(DeviceType type, string name, string address, byte[] devicedata)
        {
            using (await _lock.LockAsync())
            {
                var device = new DeviceDTO { DeviceType = type, Address = address, Name = name, DeviceData = devicedata };
                await InitAsync();
                await _databaseConnection.InsertAsync(device);
            }
        }

        public async Task DeleteDeviceAsync(DeviceType type, string address)
        {
            using (await _lock.LockAsync())
            {
                var device = await _databaseConnection.Table<DeviceDTO>().Where(d => d.DeviceType == type && d.Address == address).FirstOrDefaultAsync();
                if (device != null)
                {
                    await _databaseConnection.DeleteAsync(device);
                }
            }
        }

        public async Task DeleteDevicesAsync()
        {
            using (await _lock.LockAsync())
            {
                await _databaseConnection.ExecuteAsync("DELETE FROM Device");
            }
        }

        public async Task UpdateDeviceAsync(DeviceType type, string address, string newName)
        {
            using (await _lock.LockAsync())
            {
                var device = await _databaseConnection.Table<DeviceDTO>().Where(d => d.DeviceType == type && d.Address == address).FirstOrDefaultAsync();
                if (device != null)
                {
                    device.Name = newName;
                    await _databaseConnection.UpdateAsync(device);
                }
            }
        }
    }
}
