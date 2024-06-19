using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BrickController2.Android.UI.CustomHandlers;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.Droid.UI.Services.DI;
using BrickController2.UI.Controls;
using BrickController2.UI.DI;

namespace BrickController2.Droid
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
            : base(handle, transer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler<ColorImage, ColorImageHandler>();
                    handlers.AddHandler<ExtendedSlider, ExtendedSliderHandler>();
                })
                .ConfigureContainer(new AutofacServiceProviderFactory(), autofacBuilder =>
                {
                    autofacBuilder.RegisterInstance(this).As<Context>();
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