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

        public YamvvmViewModel model;
        public YamvvmPage modelPage;

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
            if (model != null && modelPage != null)
            {
                model.ModelReady -= modelPage.ModelReady;
            }

            //INavigation nav = ViewModelNavigation.GetNavigation();

            //if (nav == null) return;

            IReadOnlyList<Page> navStack = navStackProperty(modelPage.Navigation);

            if (navStack == null || navStack.Count == 0) return;

            Type pageType = navStack.Last().GetType();

            //if (navStack.Last() is YamvvmNavigationPage p)
            //{
            //    pageType = p.Navigation.NavigationStack.Last().GetType();
            //}

            //if (modelPage != null && pageType != modelPage.GetType()) return;

            //ViewModelNavigation.PagePopped(modelPage);
            await navPopTask(modelPage.Navigation);

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
            if (modelPage?.Parent is YamvvmNavigationPage navigationPage)
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

    public abstract class PageViewModel : YamvvmViewModel
    {
        public Command BackCommand => new Command(async () => await PopPage());

        public async Task PopPage()
        {
            //this.modelPage.SendBackButtonPressed();
            //await Task.CompletedTask;

            var modal = ViewModelNavigation.IsModal(this.modelPage);

            if (modal)
            {
                await Pop((nav) => nav.ModalStack, async (nav) => await nav.PopModalAsync());
            }
            else
            {
                await Pop((nav) => nav.NavigationStack, async (nav) => await nav.PopAsync());
            }


            //await Pop((nav) => {
            //    if (!modal && nav.NavigationStack != null && nav.NavigationStack.Count > 0)
            //        return nav.NavigationStack;
            //    else if (modal && nav.ModalStack != null && nav.ModalStack.Count > 0)
            //        return nav.ModalStack;
            //    else return null;

            //}, async (nav) => {
            //    if (!modal && nav.NavigationStack != null && nav.NavigationStack.Count > 0)
            //        await nav.PopAsync();
            //    else if (modal && nav.ModalStack != null && nav.ModalStack.Count > 0)
            //        await nav.PopModalAsync();

            //});
        }
    }
}
