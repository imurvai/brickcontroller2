using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BrickController2.DeviceManagement
{
    public interface IDeviceManager : INotifyPropertyChanged
    {
        ObservableCollection<Device> Devices { get; }
        bool IsScanning { get; }

        Task LoadDevicesAsync();
        Task StartScanAsync();
        Task StopScanAsync();
        Task<Device> GetDeviceById(string Id);
        Task DeleteDeviceAsync(Device device);
    }
}
