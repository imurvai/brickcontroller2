using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<DeviceDTO>> GetDevices();
        Task InsertDevice(DeviceDTO device);
        Task DeleteDevice(DeviceDTO device);
        Task UpdateDevice(DeviceDTO device);
    }
}
