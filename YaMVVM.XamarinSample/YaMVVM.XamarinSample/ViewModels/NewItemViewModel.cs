using F3N.YaMVVM.ViewModel;
using System;
using Xamarin.Forms;
using YaMVVM.XamarinSample.Models;
using YaMVVM.XamarinSample.Services;

namespace YaMVVM.XamarinSample.ViewModels
{
    public class NewItemViewModel : PageViewModel
    {
        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>() ?? new MockDataStore();

        public Command Save => new Command(async () => {
            await DataStore.AddItemAsync(new Item() { Id = Guid.NewGuid().ToString(), Text = Text, Description = Description });
            await PopPage();
        });

        public Command Cancel => new Command(async () => {
            await PopPage();
        });

        public string Text { get; set; }
        public string Description { get; set; }

        public NewItemViewModel(Item item = null)
        {
            Title = item?.Text;
            Text = item?.Text;
            Description = item?.Description;
        }
    }
}
