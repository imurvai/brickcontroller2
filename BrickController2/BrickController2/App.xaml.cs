using System;
using BrickController2.UI.DI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BrickController2.UI.ViewModels;
using BrickController2.UI.Pages;

[assembly: XamlCompilation (XamlCompilationOptions.Skip)]
namespace BrickController2
{
	public partial class App : Application
	{
		public App(
            ViewModelFactory viewModelFactory, 
            PageFactory pageFactory, 
            Func<Page, NavigationPage> navigationPageFactory)
		{
			InitializeComponent();

            var vm = viewModelFactory(typeof(CreationListViewModel), null);
		    var rootPage = pageFactory(typeof(CreationListPage), vm);
		    MainPage = navigationPageFactory(rootPage);
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
