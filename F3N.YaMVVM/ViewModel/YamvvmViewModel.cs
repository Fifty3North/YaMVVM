using F3N.YaMVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace F3N.YaMVVM.ViewModel
{
    public abstract class YamvvmViewModel : INotifyPropertyChanged, IViewModel
    {
        public event EventHandler<object> ModelReady;

        protected void RaiseModelReadyEvent(object sender, object args)
        {
            ModelReady?.Invoke(sender, args);
        }

        public bool Initialised { get; private set; }

        public YamvvmPage ModelPage;

        private bool isBusy = false;
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        public Thickness SafeArea { get; set; }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler changed = PropertyChanged;

            changed?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual Task Initialise()
        {
            Initialised = true;
            RaiseModelReadyEvent(this, true);
            return Task.CompletedTask;
        }

        public virtual Task OnReappearing()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDisappearing()
        {
            return Task.CompletedTask;
        }

        public virtual Task Destroy()
        {
            return Task.CompletedTask;
        }

        protected async Task Pop(Func<INavigation, IReadOnlyList<Page>> navStackProperty, Func<INavigation, Task> navPopTask)
        {
            if (ModelPage != null)
            {
                this.ModelReady -= ModelPage.ModelReady;
            }

            IReadOnlyList<Page> navStack = navStackProperty(ModelPage.Navigation);

            if (navStack == null || navStack.Count == 0) return;

            Type pageType = navStack.Last().GetType();

            await navPopTask(ModelPage.Navigation);

            await ViewModelNavigation.DoAsyncWithoutWaiting(async () =>
            {
                await Destroy();
            });
        }
    }

    public abstract class TabsViewModel : YamvvmViewModel
    {

    }

    public abstract class TabbedPageViewModel : YamvvmViewModel
    {
        protected YamvvmViewModel SwitchTab(int index)
        {
            if (ModelPage?.Parent is YamvvmNavigationPage navigationPage)
            {
                if (navigationPage.Parent is TabbedPage tabbedPage)
                {
                    if (index >= 0 && index < tabbedPage.Children.Count)
                    {
                        tabbedPage.CurrentPage = tabbedPage.Children[index];
                        return (YamvvmViewModel)((YamvvmNavigationPage)tabbedPage.CurrentPage).CurrentPage.BindingContext;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Tab index is not in range. Selected: {index}");
                    }
                }
            }

            return null;
        }
    }

    public abstract class PageViewModel : YamvvmViewModel, INotifyPropertyChanged
    {
        private bool _isPopping;

        public Command BackCommand => new Command(async () => await PopPage());

        public async Task PopPage()
        {
            if (!_isPopping)
            {
                _isPopping = true;
                var modal = ViewModelNavigation.IsModal(this.ModelPage);

                if (modal)
                {
                    await Pop((nav) => nav.ModalStack, async (nav) => await nav.PopModalAsync());
                }
                else
                {
                    await Pop((nav) => nav.NavigationStack, async (nav) => await nav.PopAsync());
                }
            }
        }

        public async Task DisplayAlert(string title, string text, string confirmText)
        {
            await this.ModelPage.DisplayAlert(title, text, confirmText);
        }

        public async Task<bool> DisplayAlert(string title, string text, string confirmText, string cancelText)
        {
            return await this.ModelPage.DisplayAlert(title, text, confirmText, cancelText);
        }
    }
}
