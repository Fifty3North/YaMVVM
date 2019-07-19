using F3N.YaMVVM.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

using YaMVVM.XamarinSample.Models;
using YaMVVM.XamarinSample.Services;
using YaMVVM.XamarinSample.Views;

namespace YaMVVM.XamarinSample.ViewModels
{
    public class ItemsViewModel : PageViewModel
    {
        public ObservableCollection<Item> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public Command AddItem { get; set; }

        public Command SelectItem { get; set; }

        public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>() ?? new MockDataStore();

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Item>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AddItem = new Command(async () => await this.PushModalPage<NewItemPage>(new NewItemViewModel(), toolbar: true));
            SelectItem = new Command(async (param) => {
                if (param is Item item)
                {
                    await this.PushPage<ItemDetailPage>(new ItemDetailViewModel(item));
                }
            });
        }

        public override async Task Initialise()
        {
            await ExecuteLoadItemsCommand();
            await base.Initialise();
        }

        public override async Task OnReappearing()
        {
            await GetNewItems();
            await base.OnReappearing();
        }

        async Task GetNewItems()
        {
            var items = await DataStore.GetItemsAsync(true);

            foreach (var item in items)
            {
                if (!Items.Any(i => i.Id == item.Id))
                {
                    Items.Add(item);
                }
            }
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}