using BrickController2.Windows.PlatformServices.BluetoothLE;
using BrickController2.Windows.PlatformServices.Infrared;
using BrickController2.Windows.PlatformServices.Versioning;
using BrickController2.Windows.PlatformServices.Localization;
using BrickController2.Windows.PlatformServices.GameController;
using BrickController2.Windows.PlatformServices.SharedFileStorage;
using BrickController2.Windows.PlatformServices.Permission;
using Autofac;
using BrickController2.PlatformServices.Infrared;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.Versioning;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.Localization;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.PlatformServices.Permission;

namespace BrickController2.Windows.PlatformServices.DI;

public class PlatformServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
        builder.RegisterType<GameControllerService>().AsSelf().As<IGameControllerService>().SingleInstance();
        builder.RegisterType<VersionService>().As<IVersionService>().SingleInstance();
        builder.RegisterType<BleService>().As<IBluetoothLEService>().SingleInstance();
        builder.RegisterType<LocalizationService>().As<ILocalizationService>().SingleInstance();
        builder.RegisterType<SharedFileStorageService>().As<ISharedFileStorageService>().SingleInstance();
        builder.RegisterType<ReadWriteExternalStoragePermission>().As<IReadWriteExternalStoragePermission>().InstancePerDependency();
        builder.RegisterType<BluetoothPermission>().As<IBluetoothPermission>().InstancePerDependency();
    }
}