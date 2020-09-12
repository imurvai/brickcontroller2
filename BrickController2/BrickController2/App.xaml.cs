using System;
using BrickController2.UI.DI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrickController2.UI.ViewModels;
using BrickController2.UI.Pages;
using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Preferences;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
namespace BrickController2
{
	public partial class App
	{
        private readonly BackgroundService _backgroundService;
		private readonly IPreferencesService _preferencesService;
		private readonly IThemeService _themeService;

		public App(
            ViewModelFactory viewModelFactory, 
            PageFactory pageFactory, 
            Func<Page, NavigationPage> navigationPageFactory,
            BackgroundService backgroundService,
			IPreferencesService preferencesService,
			IThemeService themeService)
		{
			InitializeComponent();

            _backgroundService = backgroundService;
			_preferencesService = preferencesService;
			_themeService = themeService;

			RequestedThemeChanged += (s, e) => ApplyCurrentTheme();
			ApplyCurrentTheme();

            var vm = viewModelFactory(typeof(CreationListPageViewModel), null);
		    var page = pageFactory(typeof(CreationListPage), vm);
		    var navigationPage = navigationPageFactory(page);
            navigationPage.BarBackgroundColor = Color.Red;
            navigationPage.BarTextColor = Color.White;

            MainPage = navigationPage;
		}

		protected override void OnStart()
		{
		}

		protected override void OnSleep()
		{
            _backgroundService.FireApplicationSleepEvent();
		}

		protected override void OnResume()
		{
            _backgroundService.FireApplicationResumeEvent();
		}

		private void ApplyCurrentTheme()
        {
			var currentTheme = _preferencesService.Get("Theme", ThemeType.System);
			_themeService.ApplyTheme(currentTheme);
		}
	}
}
