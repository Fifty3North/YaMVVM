using F3N.YaMVVM.App;
using F3N.YaMVVM.ViewModel;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using YaMVVM.XamarinSample.Services;
using YaMVVM.XamarinSample.ViewModels;
using YaMVVM.XamarinSample.Views;

namespace YaMVVM.XamarinSample
{
    public partial class App : YamvvmApp
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            _ = ViewModelNavigation.SetTabbedMainPage<MainPage>();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
