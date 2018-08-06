using System.Collections.Generic;
using Autofac;
using BrickController2.UI.Navigation;
using BrickController2.UI.Pages;
using BrickController2.UI.ViewModels;
using Xamarin.Forms;

namespace BrickController2.UI.DI
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register services

            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();

            // Register pages and viewmodels

            builder.RegisterType<CreationListViewModel>().Keyed<ViewModelBase>(typeof(CreationListViewModel));
            builder.RegisterType<CreationListPage>().Keyed<PageBase>(typeof(CreationListPage));

            builder.RegisterType<CreationDetailsViewModel>().Keyed<ViewModelBase>(typeof(CreationDetailsViewModel));
            builder.RegisterType<CreationDetailsPage>().Keyed<PageBase>(typeof(CreationDetailsPage));

            builder.RegisterType<DeviceListViewModel>().Keyed<ViewModelBase>(typeof(DeviceListViewModel));
            builder.RegisterType<DeviceListPage>().Keyed<PageBase>(typeof(DeviceListPage));

            // Register the viewmodel factory
            builder.Register<ViewModelFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (type, parameters) => componentContext.ResolveKeyed<ViewModelBase>(type, new TypedParameter(typeof(IDictionary<string, object>), parameters));
            });

            // Register the page factory
            builder.Register<PageFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (type, vm) => componentContext.ResolveKeyed<PageBase>(type, new TypedParameter(typeof(ViewModelBase), vm));
            });

            // 
            builder.RegisterType<NavigationPage>();
            builder.RegisterType<App>();
        }
    }
}
