using System.Threading.Tasks;
using LibrePay.ViewModels.Base;
using Xamarin.Forms;

namespace LibrePay.Interfaces.Services.Navigation
{
    public interface INavigationService
    {
        Page PreviousPage { get; }

        BaseViewModel PreviousPageViewModel { get; }

        Task InitializeAsync<TViewModel>() where TViewModel : BaseViewModel;

        // Page

        Task NavigateToAsync<TViewModel>(params object[] parameters) where TViewModel : BaseViewModel;

        Task PopStackAsync();

        Task ClearStack();

        // Modal

        Task NavigateToModalAsync<TViewModel>(params object[] parameters) where TViewModel : BaseViewModel;

        Task PopModalStackAsync();

        Task ClearModalStack();
    }
}
