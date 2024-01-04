using Autofac;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Pages;
using BrickController2.UI.ViewModels;
using BrickController2.UI.Services.MainThread;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.Services.Theme;

namespace BrickController2.UI.DI
{
    public class UiModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Register services

            builder.RegisterType<NavigationService>().As<INavigationService>().SingleInstance();
            builder.RegisterType<MainThreadService>().As<IMainThreadService>().SingleInstance();
            builder.RegisterType<BackgroundService>().AsSelf().As<IBackgroundService>().SingleInstance();
            builder.RegisterType<TranslationService>().AsSelf().As<ITranslationService>().SingleInstance();
            builder.RegisterType<PreferencesService>().AsSelf().As<IPreferencesService>().SingleInstance();
            builder.RegisterType<ThemeService>().AsSelf().As<IThemeService>().SingleInstance();

            // Register Dialogs
            builder.RegisterType<DialogService>().As<IDialogService>().As<IDialogServerHost>().SingleInstance();

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

            // Xamarin forms related
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
