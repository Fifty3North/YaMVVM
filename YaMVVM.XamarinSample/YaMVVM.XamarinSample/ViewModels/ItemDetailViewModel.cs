using F3N.YaMVVM.ViewModel;
using System;
using Xamarin.Forms;
using YaMVVM.XamarinSample.Models;
using YaMVVM.XamarinSample.Services;

namespace YaMVVM.XamarinSample.ViewModels
{
    public class ItemDetailViewModel : PageViewModel
    {
        public string Text { get; set; }
        public string Description { get; set; }

        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Text = item?.Text;
            Description = item?.Description;
        }
    }
}
