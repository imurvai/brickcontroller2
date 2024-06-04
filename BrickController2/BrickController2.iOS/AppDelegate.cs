﻿using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Foundation;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.iOS.PlatformServices.DI;
using BrickController2.iOS.UI.Services.DI;
using BrickController2.UI.DI;

namespace BrickController2.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
                })
                .ConfigureContainer(new AutofacServiceProviderFactory(), autofacBuilder =>
                {
                    autofacBuilder.RegisterModule(new PlatformServicesModule());
                    autofacBuilder.RegisterModule(new UIServicesModule());

                    autofacBuilder.RegisterModule(new BusinessLogicModule());
                    autofacBuilder.RegisterModule(new DatabaseModule());
                    autofacBuilder.RegisterModule(new CreationManagementModule());
                    autofacBuilder.RegisterModule(new DeviceManagementModule());
                    autofacBuilder.RegisterModule(new UiModule());
                });

            return builder.Build();
        }
    }
}
