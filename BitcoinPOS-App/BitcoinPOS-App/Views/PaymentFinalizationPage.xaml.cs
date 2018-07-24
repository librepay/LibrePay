using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.ViewModels;
using Xamarin.Essentials;
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

        #region Overrides

        protected override void OnAppearing()
        {
            base.OnAppearing();

            CopyAddressToClipboard();

            StartBackgroundJob();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            StopBackgroundJob();
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

        #endregion

        #region Events

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await ExitPageAsync();
        }

        #endregion

        #region Page Logic

        private async Task AcceptPaymentAsync(decimal amount)
        {
            _viewModel.Payment.Done = true;

            await _msgDisplayer.ShowMessageAsync($"Pagamento efetuado!\nPago: {amount:N8}");
            await ExitPageAsync();
        }

        private async Task ExitPageAsync()
        {
            StopBackgroundJob();

            await Navigation.PopModalAsync();
        }

        private void CopyAddressToClipboard()
        {
            Clipboard.SetText(_viewModel.Payment.Address);
            _msgDisplayer.ShowMessageAsync("Copiado endereço");
        }

        #endregion

        #region BackgroundJob

        private void StartBackgroundJob()
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Payment.Address))
                return;

            StopBackgroundJob();

            _backgroundJob = _netInfoProvider.WaitCompletePayment(
                _viewModel.Payment,
                amount => Device.BeginInvokeOnMainThread(async () => await AcceptPaymentAsync(amount))
            );
        }

        private void StopBackgroundJob()
        {
            _backgroundJob?.Cancel();
        }

        #endregion
    }
}