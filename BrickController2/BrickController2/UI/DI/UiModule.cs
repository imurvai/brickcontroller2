using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Pages;
using BrickController2.UI.ViewModels;
using Xamarin.Forms;
using BrickController2.UI.Services.UIThread;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.DI
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register services

            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<UIThreadService>().As<IUIThreadService>().SingleInstance();
            builder.RegisterType<BackgroundService>().AsSelf().As<IBackgroundService>().SingleInstance();
            builder.RegisterType<TranslationService>().AsSelf().As<ITranslationService>().SingleInstance();

            // Register viewmodels
            foreach (var vmType in GetSubClassesOf<PageViewModelBase>())
            {
                builder.RegisterType(vmType).Keyed<PageViewModelBase>(vmType);
            }

            // Register pages
            foreach (var pageType in GetSubClassesOf<PageBase>())
            {
                builder.RegisterType(pageType).Keyed<PageBase>(pageType);
            }

            // Register the viewmodel factory
            builder.Register<ViewModelFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (type, parameters) => componentContext.ResolveKeyed<PageViewModelBase>(type, new TypedParameter(typeof(NavigationParameters), parameters));
            });

            // Register the page factory
            builder.Register<PageFactory>(c =>
            {
                var componentContext = c.Resolve<IComponentContext>();
                return (type, vm) => componentContext.ResolveKeyed<PageBase>(type, new TypedParameter(typeof(PageViewModelBase), vm));
            });

            // 
            builder.RegisterType<NavigationPage>();
            builder.RegisterType<App>();
        }

        private IEnumerable<Type> GetSubClassesOf<T>()
        {
            return ThisAssembly.GetTypes()
                .Where(t => t != typeof(T) && typeof(T).IsAssignableFrom(t))
                .ToList();
        }
    }
}
