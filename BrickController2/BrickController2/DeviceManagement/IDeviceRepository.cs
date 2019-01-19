using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    internal interface IDeviceRepository
    {
        Task<IEnumerable<DeviceDTO>> GetDevicesAsync();
        Task InsertDeviceAsync(DeviceType type, string name, string address, byte[] manufacturerData);
        Task DeleteDeviceAsync(DeviceType type, string address);
        Task DeleteDevicesAsync();
        Task UpdateDeviceAsync(DeviceType type, string address, string newName);
    }
}
