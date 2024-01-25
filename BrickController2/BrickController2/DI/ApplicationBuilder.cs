using Autofac.Extensions.DependencyInjection;
using Autofac;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.UI.DI;
using ZXing.Net.Maui.Controls;

namespace BrickController2.DI;

public static class ApplicationBuilder
{
    public static MauiAppBuilder Create()
    {
        var builder = MauiApp.CreateBuilder();

        return builder
            // configure BC2 app
            .UseMauiApp<App>()
            // configure other common dependencies if needed
            .UseBarcodeReader();
            ;
    }

    public static MauiAppBuilder ConfigureContainer(this MauiAppBuilder builder, Action<ContainerBuilder> configure)
    {
        builder.ConfigureContainer(new AutofacServiceProviderFactory(), (containerBuilder) =>
        {
            // BrickController core DI setup
            containerBuilder.RegisterModule<BusinessLogicModule>();
            containerBuilder.RegisterModule<DatabaseModule>();
            containerBuilder.RegisterModule<CreationManagementModule>();
            containerBuilder.RegisterModule<DeviceManagementModule>();
            containerBuilder.RegisterModule<UiModule>();
            // execute per platform configuration
            configure(containerBuilder);
        });
        return builder;
    }
}
