using F3N.YaMVVM.Core.Extensions;
using F3N.YaMVVM.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace F3N.YaMVVM.ViewModel
{
    internal class PageInfo
    {
        internal YamvvmPage Page;
        internal bool IsModal;
    }

    public static class ViewModelNavigation
    {
        private static IErrorHandler _errorHandler;

        private static Stack<PageInfo> _pageHandler;

        static ViewModelNavigation()
        {
            _errorHandler = new DefaultErrorHandler();
            _pageHandler = new Stack<PageInfo>();
        }

        public static async Task DoAsyncWithoutWaiting(Func<Task> func, bool runOnNewThread = true)
        {
            Task task = (runOnNewThread)
                ? Task.Run(func)
                : func();

#if (DEBUG)
            await task;
#else
            task.FireAndForgetSafeAsync(_errorHandler);
#endif
        }

        public static INavigation GetNavigation()
        {
            switch (Application.Current.MainPage)
            {
                case MultiPage<Page> mp:
                    return mp.CurrentPage.Navigation;
                case MasterDetailPage main:
                    return main.Detail.Navigation;
                case Page p:
                    return p.Navigation;
                default:
                    System.Diagnostics.Debug.WriteLine("Unable to determine Navigation");
                    return null;
            }
        }

        /// <summary>
        /// Sets MainPage to a page.
        /// Also initializes the page view model if provided.
        /// </summary>
        /// <typeparam name="TPage">Must be of type YamvvmPage.</typeparam>
        /// <param name="vm"></param>
        public static async Task SetMainPage<TPage>(PageViewModel vm = null) where TPage : YamvvmPage, new()
        {
            TPage page = new TPage();

            Task initialiseViewModelTask = InitialisePage<TPage>(page, vm);

            YamvvmNavigationPage navigationPage = new YamvvmNavigationPage(page);

            await CleanupPreviousMainPage();

            Device.BeginInvokeOnMainThread(() => Application.Current.MainPage = navigationPage);

            await initialiseViewModelTask;
        }

        /// <summary>
        /// Set MainPage to a tabbed page container.
        /// Also initializes all tabs' viewmodels.
        /// </summary>
        /// <typeparam name="TPage">Must be of type TabbedPage.</typeparam>
        /// <returns></returns>
        public static async Task SetTabbedMainPage<TPage>(int pageIndex = 0, TabbedPageViewModel tabsViewModel = null, PageViewModel[] tabViewModels = null) where TPage : TabbedPage, new()
        {
            TPage tabbedPage = new TPage();

            if (tabsViewModel != null)
            {
                tabbedPage.BindingContext = tabsViewModel;
            }

            if(tabViewModels != null)
            {
                for (int tabIndex = 0; tabIndex < tabbedPage.Children.Count; tabIndex++)
                {
                    if(tabViewModels.Length > tabIndex)
                    {
                        tabbedPage.Children[tabIndex].BindingContext = tabViewModels[tabIndex];
                    }
                }
            }

            var page = tabbedPage.Children[pageIndex];
            tabbedPage.CurrentPage = page;

            Device.BeginInvokeOnMainThread(() =>
            {
                Application.Current.MainPage = tabbedPage;
            });

            await CleanupPreviousMainPage();
            await Init(page);

            var initTasks = new List<Task>();

            int index = 0;

            foreach (var cp in tabbedPage.Children)
            {
                if (index != pageIndex)
                {
                    initTasks.Add(Init(cp));
                }
                index++;
            };

            await Task.WhenAll(initTasks);
        }

        private static async Task Init(Page cp)
        {
            if (cp is YamvvmNavigationPage nav)
            {
                if (nav.CurrentPage is YamvvmPage page)
                {
                    if (page.BindingContext is YamvvmViewModel vm)
                    {
                        await InitialisePageViewModel(page, vm);
                    }
                }
            }
        }

        internal static bool IsModal(YamvvmPage modelPage)
        {
            if(_pageHandler.Count > 0 && modelPage == _pageHandler.Peek().Page)
            {
                return _pageHandler.Peek().IsModal;
            }

            return false;
        }

        internal static void PagePopped(YamvvmPage modelPage)
        {
            if (_pageHandler.Count > 0 && modelPage == _pageHandler.Peek().Page)
            {
                _pageHandler.Pop();
            }
        }

        /// <summary>
        /// Push new NON-DETAIL page onto the navigation stack.
        /// Only use when no master/detail page has been added.
        /// </summary>
        /// <param name="vm">The viewmodel to bind to the page.</param>
        /// <typeparam name="TPage">Must be of type YamvvmPage.</typeparam>
        /// <returns></returns>
        public static async Task PushPage<TPage>(this PageViewModel currentViewModel, PageViewModel vm = null) where TPage : YamvvmPage, new()
        {
            await Push<TPage>(currentViewModel, vm, async (nav, page) => {
                _pageHandler.Push(new PageInfo { Page = page, IsModal = false });
                await nav.PushAsync(page);
            });
        }

        public static async Task PushModalPage<TPage>(this PageViewModel currentViewModel, PageViewModel vm = null, bool toolbar = false) where TPage : YamvvmPage, new()
        { 
            await Push<TPage>(currentViewModel, vm, async (nav, page) => {
                _pageHandler.Push(new PageInfo { Page = page, IsModal = true });
                await (toolbar ? nav.PushModalAsync(new YamvvmNavigationPage(page)) : nav.PushModalAsync(page));
            });
        }

        private static async Task Push<TPage>(PageViewModel currentViewModel, YamvvmViewModel vm, Func<INavigation, TPage, Task> pushTask) where TPage : YamvvmPage, new()
        {
            TPage page = new TPage();

            Task initialiseViewModelTask = InitialisePage(page, vm);
            INavigation navigation;

            if (currentViewModel.modelPage.Navigation == null)
            {
                navigation = GetNavigation();
            }
            else
            {
                //navigation = GetNavigation();
                navigation = currentViewModel.modelPage.Navigation;
            }

            if (navigation != null)
            {
                Device.BeginInvokeOnMainThread(async () => await pushTask(navigation, page));
            }

            await initialiseViewModelTask;
        }

        public static async Task PopupAlert(string titleText, string message, string cancel)
        {
            Device.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.DisplayAlert(titleText, message, cancel));
            await Task.CompletedTask;
        }

        public static async Task<bool> PopupAlert(string titleText, string message, string accept, string cancel)
        {
            return await Application.Current.MainPage.DisplayAlert(titleText, message, accept, cancel);
        }

        private static async Task InitialisePage<TPage>(TPage page, YamvvmViewModel vm) where TPage : YamvvmPage, new()
        {
            if (vm != null)
            {
                page.BindingContext = vm;

                await InitialisePageViewModel(page, vm);
            }
        }

        private static async Task InitialisePageViewModel(YamvvmPage page, YamvvmViewModel vm)
        {
            if (page != null && vm != null)
            {
                vm.modelPage = page;
                vm.model = vm;
                vm.ModelReady += page.ModelReady;

                
#if DEBUG
                    var vmtype = vm.GetType().Name;
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    System.Diagnostics.Debug.WriteLine("Starting init of VM {0}", vmtype);
#endif

                    await vm.Initialise();

#if DEBUG
                    sw.Stop();
                    System.Diagnostics.Debug.WriteLine("Finished init of VM {0} ({1}ms)", vmtype, sw.ElapsedMilliseconds);
#endif
                
            }
        }

        private static async Task CleanupPreviousMainPage()
        {
#if DEBUG
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var vmtype = Application.Current.MainPage?.GetType().Name;
            System.Diagnostics.Debug.WriteLine("Starting CleanupPreviousMainPage {0}", vmtype);
#endif

            if (Application.Current.MainPage is MultiPage<Page> mp)
            {
                foreach (Page cp in mp.Children)
                {
                    if (cp is YamvvmNavigationPage np)
                    {
                        await DestroyNavigationPageViewModel(np);
                    }
                    else if (cp is YamvvmPage bp)
                    {
                        await DestroyPageViewModel(bp);
                    }
                }
            }
            else if (Application.Current.MainPage is YamvvmNavigationPage np)
            {
                await DestroyNavigationPageViewModel(np);
            }
            else if (Application.Current.MainPage is Page p)
            {
                if (p is YamvvmPage bp)
                {
                    await DestroyPageViewModel(bp);
                }
            }

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Finished CleanupPreviousMainPage {0} ({1}ms)", vmtype, sw.ElapsedMilliseconds);
#endif
        }

        private static async Task DestroyNavigationPageViewModel(NavigationPage navigationPage)
        {
            var taskList = new List<Task>();

            foreach (Page page in navigationPage.Navigation.NavigationStack)
            {
                if (page is YamvvmPage bp)
                {
                    taskList.Add(DestroyPageViewModel(bp));
                }
            }

            await Task.WhenAll(taskList);
        }

        private static async Task DestroyPageViewModel(YamvvmPage page)
        {
            if (page.BindingContext is YamvvmViewModel vm)
            {
                if (vm.model != null && vm.modelPage != null)
                {
                    vm.model.ModelReady -= vm.modelPage.ModelReady;
                }

                await DoAsyncWithoutWaiting(async () =>
                {
                    await vm.Destroy();
                });
            }
        }
    }
}
