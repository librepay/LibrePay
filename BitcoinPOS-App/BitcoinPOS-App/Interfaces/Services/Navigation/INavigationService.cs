using System.Threading.Tasks;
using BitcoinPOS_App.ViewModels.Base;
using Xamarin.Forms;

namespace BitcoinPOS_App.Interfaces.Services.Navigation
{
    public interface INavigationService
    {
        Page PreviousPage { get; }

        BaseViewModel PreviousPageViewModel { get; }

        Task InitializeAsync<TViewModel>() where TViewModel : BaseViewModel;

        // Page

        Task NavigateToAsync<TViewModel>(params object[] parameter) where TViewModel : BaseViewModel;

        Task PopStackAsync();

        Task ClearStack();

        // Modal

        Task NavigateToModalAsync<TViewModel>(params object[] parameters) where TViewModel : BaseViewModel;

        Task PopModalStackAsync();

        Task ClearModalStack();
    }
}
