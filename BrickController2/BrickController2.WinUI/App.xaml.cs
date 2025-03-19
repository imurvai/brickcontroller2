using Autofac;
using Autofac.Extensions.DependencyInjection;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.UI.Controls;
using BrickController2.UI.DI;
using BrickController2.UI.Pages;
using BrickController2.Windows.PlatformServices.DI;
using BrickController2.Windows.UI.CustomHandlers;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Windows.Devices.Input;

namespace BrickController2.Windows;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }
    protected override MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<BrickController2.App>()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers
                    .AddHandler<ExtendedSlider, ExtendedSliderHandler>()
                    .AddHandler<PageBase, CustomPageHandler>()
                ;

                // handle swipe if there is no touch screen
                var capablitities = new TouchCapabilities();
                if (capablitities.TouchPresent == 0)
                {
                    handlers.AddHandler<SwipeView, CustomSwipeViewHandler>();
                }
            })
            .ConfigureContainer(new AutofacServiceProviderFactory(), autofacBuilder =>
            {
                autofacBuilder.RegisterModule<PlatformServicesModule>();
                autofacBuilder.RegisterModule<BusinessLogicModule>();
                autofacBuilder.RegisterModule<DatabaseModule>();
                autofacBuilder.RegisterModule<CreationManagementModule>();
                autofacBuilder.RegisterModule<DeviceManagementModule>();
                autofacBuilder.RegisterModule<UiModule>();
            });

        return builder.Build();
    }
}
