using System;
using BrickController2.UI.DI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrickController2.UI.ViewModels;
using BrickController2.UI.Pages;
using BrickController2.Helpers;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
namespace BrickController2
{
	public partial class App
	{
		public App(
            ViewModelFactory viewModelFactory, 
            PageFactory pageFactory, 
            Func<Page, NavigationPage> navigationPageFactory)
		{
			InitializeComponent();
            ThreadHelper.Init();

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
		}

		protected override void OnResume()
		{
		}
	}
}
