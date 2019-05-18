using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using LibrePay.Interfaces.Services.Navigation;
using LibrePay.ViewModels.Base;
using Plugin.Iconize;
using Xamarin.Forms;

namespace LibrePay.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        public virtual BaseViewModel PreviousPageViewModel
            => PreviousPage.BindingContext as BaseViewModel;

        public virtual Page PreviousPage
            => MainPage.Navigation.NavigationStack[MainPage.Navigation.NavigationStack.Count - 1];

        protected virtual NavigationPage MainPage
        {
            get => Application.Current.MainPage as NavigationPage;
            set => Application.Current.MainPage = value;
        }

        public virtual Task InitializeAsync<TViewModel>() where TViewModel : BaseViewModel
        {
            var mainPage = CreatePage(typeof(TViewModel));
            MainPage = new IconNavigationPage(mainPage);

            return Task.FromResult(true);
        }

        #region Page

        public virtual Task NavigateToAsync<TViewModel>(params object[] parameters) where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), false, parameters);
        }

        public virtual Task PopStackAsync()
        {
            if (MainPage.Navigation.NavigationStack.Count < 1)
                return Task.FromResult(true);

            RunOnMainThread(async () =>
            {
                await MainPage.Navigation.PopAsync();
            });

            return Task.FromResult(true);
        }

        public virtual Task ClearStack()
        {
            RunOnMainThread(async () =>
            {
                await MainPage.Navigation.PopToRootAsync();
            });

            return Task.FromResult(true);
        }

        #endregion

        #region Modal

        public virtual Task NavigateToModalAsync<TViewModel>(params object[] parameters)
            where TViewModel : BaseViewModel
        {
            return InternalNavigateToAsync(typeof(TViewModel), true, parameters);
        }

        public virtual Task PopModalStackAsync()
        {
            if (MainPage.Navigation.ModalStack.Count < 1)
                return Task.FromResult(true);

            RunOnMainThread(async () =>
            {
                await MainPage.Navigation.PopModalAsync();
            });

            return Task.FromResult(true);
        }

        public virtual Task ClearModalStack()
        {
            var len = MainPage.Navigation.ModalStack.Count - 1;
            RunOnMainThread(async () =>
            {
                for (var i = 0; i < len; i++)
                    await MainPage.Navigation.PopModalAsync();
            });

            return Task.FromResult(true);
        }

        #endregion

        protected virtual async Task InternalNavigateToAsync(Type viewModelType, bool isModal, object[] parameters)
        {
            var page = CreatePage(viewModelType);

            Debug.Assert(page.BindingContext != null, "page.BindingContext != null");
            await ((BaseViewModel)page.BindingContext)
                .InitializeAsync(parameters);

            RunOnMainThread(async () =>
            {
                if (isModal)
                    await MainPage.Navigation.PushModalAsync(page);
                else
                    await MainPage.PushAsync(page);
            });
        }

        protected virtual Type GetPageTypeForViewModel(Type viewModelType)
        {
            Debug.Assert(viewModelType.FullName != null, "viewModelType.FullName != null");
            var viewNameSplit = viewModelType.FullName.Split('.');
            viewNameSplit[viewNameSplit.Length - 1] = viewNameSplit[viewNameSplit.Length - 1]
                .Replace("ViewModel", string.Empty);
            viewNameSplit[viewNameSplit.Length - 2] = viewNameSplit[viewNameSplit.Length - 2]
                .Replace("Model", string.Empty);

            var viewModelAssemblyName = viewModelType.GetTypeInfo().Assembly.FullName;

            var viewAssemblyName = string.Format(
                CultureInfo.InvariantCulture
                , "{0}, {1}"
                , string.Join(".", viewNameSplit)
                , viewModelAssemblyName
            );
            var viewType = Type.GetType(viewAssemblyName);

            return viewType;
        }

        protected virtual Page CreatePage(Type viewModelType)
        {
            var pageType = GetPageTypeForViewModel(viewModelType);
            if (pageType == null)
            {
                throw new TypeInitializationException(
                    viewModelType.FullName
                    , new Exception($"Cannot locate page type for {viewModelType}")
                );
            }

            var container = GetApplicationContainer();
            var page = (Page)container.Resolve(pageType);

            return page;
        }

        protected virtual ILifetimeScope GetApplicationContainer()
        {
            return App.Container;
        }

        protected virtual void RunOnMainThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Device.BeginInvokeOnMainThread(action);
        }
    }
}
