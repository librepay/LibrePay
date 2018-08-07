using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Devices;
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

        public PaymentFinalizationPage(PaymentFinalizationViewModel viewModel, IMessageDisplayer msgDisplayer)
        {
            InitializeComponent();

            _msgDisplayer = msgDisplayer;

            BindingContext = _viewModel = viewModel;

            MessagingCenter.Subscribe<PaymentFinalizationViewModel, decimal>(
                _viewModel
                , MessengerKeys.PaymentFullyReceived
                , (_, value) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await _msgDisplayer.ShowMessageAsync($"Pagamento efetuado!\nPago: {value:N8}");
                        await ExitPageAsync();
                    });
                }
            );
            MessagingCenter.Subscribe<PaymentFinalizationViewModel, (decimal totalValue, decimal txValue)>(
                _viewModel
                , MessengerKeys.PaymentPartiallyReceived
                , (_, partialPayment) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                        _msgDisplayer.ShowMessageAsync(
                            $"Valor recebido: {partialPayment.totalValue:N8}\n" +
                            $"Aguardando o restante ({(_viewModel.Payment.ValueBitcoin - partialPayment.txValue):N8})..."
                        )
                    );
                }
            );
        }

        #region Overrides

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.CopyToClipboard(_viewModel.Payment.Address);
            _msgDisplayer.ShowMessageAsync("Copiado endereço");

            _viewModel.StartBackgroundJob();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _viewModel.StopBackgroundJob();
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

        private async void LabelCopy_Clicked(object sender, EventArgs e)
        {
            _viewModel.CopyToClipboard(((Label) sender).Text);
            await _msgDisplayer.ShowMessageAsync("Copiado");
        }

        #endregion

        public async Task ExitPageAsync()
        {
            _viewModel.StopBackgroundJob();

            await Navigation.PopModalAsync();

            MessagingCenter.Send(this, MessengerKeys.MainFinishPayment);
        }
    }
}