using System.Collections.Generic;
using Autofac;
using BrickController2.UI.Navigation;
using BrickController2.UI.Pages;
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
            builder.RegisterType<CreationListPage>().Keyed<Page>(NavigationKey.CreationList);

            builder.RegisterType<CreationDetailsViewModel>().Keyed<ViewModelBase>(typeof(CreationDetailsViewModel));
            builder.RegisterType<CreationDetailsPage>().Keyed<Page>(NavigationKey.CreationDetails);

            builder.RegisterType<DeviceListViewModel>().Keyed<ViewModelBase>(typeof(DeviceListViewModel));
            builder.RegisterType<DeviceListPage>().Keyed<Page>(NavigationKey.DeviceList);

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
                return (key, parameters) => componentContext.ResolveKeyed<Page>(key, new TypedParameter(typeof(IDictionary<string, object>), parameters));
            });

            // 
            builder.RegisterType<NavigationPage>();
            builder.RegisterType<App>();
        }
    }
}
