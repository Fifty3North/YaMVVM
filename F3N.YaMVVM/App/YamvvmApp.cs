using F3N.YaMVVM.ViewModel;
using F3N.YaMVVM.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace F3N.YaMVVM.App
{
    public class YamvvmApp : Application
    {
        public YamvvmApp()
        {
            ModalPopped += OnModalPopped;
        }

        private void OnModalPopped(object sender, ModalPoppedEventArgs e)
        {
            if (e.Modal is YamvvmNavigationPage navPage)
            {
                if (navPage.CurrentPage is YamvvmPage page)
                {
                    ViewModelNavigation.PagePopped(page);
                }
            }
        }
    }
}
