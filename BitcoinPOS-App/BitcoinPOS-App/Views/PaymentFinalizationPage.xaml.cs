using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaymentFinalizationPage
    {
        private readonly PaymentFinalizationViewModel _viewModel;
        private readonly IMessageDisplayer _msgDisplayer;
        private readonly INetworkInfoProvider _netInfoProvider;
        private BackgroundJob _backgroundJob;

        public PaymentFinalizationPage(PaymentFinalizationViewModel viewModel)
        {
            InitializeComponent();

            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();
            _netInfoProvider = DependencyService.Get<INetworkInfoProvider>();

            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrWhiteSpace(_viewModel.Payment.Address))
            {
                _backgroundJob = _netInfoProvider.WaitAddressReceiveAnyTransactionAsync(
                    _viewModel.Payment.Address,
                    () =>
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            _viewModel.Payment.Done = true;

                            await _msgDisplayer.ShowMessageAsync("Pagamento efetuado!");
                            await ExitPageAsync();
                        });
                    }
                );
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // while the payment isn't finished
            // don't let the user go back
            if (_viewModel.Payment.Done)
            {
                return base.OnBackButtonPressed();
            }

            _msgDisplayer.ShowMessageAsync("Pagamento ainda não confirmado...");
            return true;
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await ExitPageAsync();
        }

        private async void Ok_Clicked(object sender, EventArgs e)
        {
            await ExitPageAsync();
        }

        private async Task ExitPageAsync()
        {
            _backgroundJob?.Cancel();

            await Navigation.PopModalAsync();
        }
    }
}