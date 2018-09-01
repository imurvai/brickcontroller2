using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IDeviceManager
    {
        ObservableCollection<Device> Devices { get; }

        Task LoadDevicesAsync();
        Task ScanAsync(CancellationToken token);

        Task<Device> GetDeviceById(string Id);

        Task DeleteDeviceAsync(Device device);
    }
}
