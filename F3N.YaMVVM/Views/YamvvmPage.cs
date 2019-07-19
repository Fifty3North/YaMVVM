using F3N.YaMVVM.Core.Extensions;
using F3N.YaMVVM.ViewModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using NavigationPage = Xamarin.Forms.NavigationPage;
using Page = Xamarin.Forms.Page;

namespace F3N.YaMVVM.Views
{
    public class YamvvmNavigationPage : NavigationPage
    {
        public YamvvmNavigationPage()
        {

        }

        public YamvvmNavigationPage(Page page) : base(page)
        {
            //Popped += YamvvmNavigationPage_Popped;
            PopRequested += YamvvmNavigationPage_PopRequested;
        }


        private void YamvvmNavigationPage_PopRequested(object sender, global::Xamarin.Forms.Internals.NavigationRequestedEventArgs e)
        {
            if (e.Page is YamvvmPage page)
            {
                ViewModelNavigation.PagePopped(page);
            }
        }
    }

    public class YamvvmPage : ContentPage
    {
        private int _appearanceCount;
        private IErrorHandler _errorHandler;

        public YamvvmPage()
        {
            _errorHandler = new DefaultErrorHandler();
        }

        public virtual void ModelReady(object sender, object e)
        {
            Task.CompletedTask.FireAndForgetSafeAsync(_errorHandler);
        }

        //provides faster page load

        protected override void OnAppearing()
        {
            if (++_appearanceCount > 1)
            {
                if (this.BindingContext is IViewModel viewmodel)
                {
                    ViewModelNavigation.DoAsyncWithoutWaiting(async () =>
                    {
                        if (viewmodel != null)
                        {
                            await viewmodel.OnReappearing();
                        }
                    }).FireAndForgetSafeAsync(_errorHandler);
                }
            }

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            if (this.BindingContext is IViewModel viewmodel)
            {
                ViewModelNavigation.DoAsyncWithoutWaiting(async () =>
                {
                    if (viewmodel != null)
                    {
                        await viewmodel.OnDisappearing();
                    }
                }).FireAndForgetSafeAsync(_errorHandler);
            }

            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            // Determine if we at the root and if so, ask them if they want to exit the application
            if (Navigation.ModalStack.Count == 0 && Navigation.NavigationStack.Count == 1 && Navigation.NavigationStack[0].GetType() == this.GetType())
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool exit = await DisplayAlert("Exit application", "Are you sure you want to exit the application?", "Yes", "No");
                    if (exit)
                    {
                        F3N.YaMVVM.Core.DependencyServices.ICloseApplication closer = DependencyService.Get<F3N.YaMVVM.Core.DependencyServices.ICloseApplication>();
                        if (closer != null)
                        {
                            closer.Close();
                        }
                        else
                        {
                            System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
                        }
                    }
                });

                return true;
            }
            else
            {
                return base.OnBackButtonPressed();
            }
        }
    }
}