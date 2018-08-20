using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<DeviceDTO>> GetDevicesAsync();
        Task InsertDeviceAsync(DeviceDTO device);
        Task DeleteDeviceAsync(DeviceDTO device);
        Task UpdateDeviceAsync(DeviceDTO device);
    }
}
