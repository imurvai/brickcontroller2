using System;
using BrickController2.UI.Navigation;
using BrickController2.UI.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace BrickController2
{
	public partial class App : Application
	{
		public App(PageFactory pageFactory, Func<Page, NavigationPage> navigationPageFactory)
		{
			InitializeComponent();

		    var rootPage = pageFactory(NavigationKey.CreationList, null);
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
