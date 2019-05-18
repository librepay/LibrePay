using System;
using System.Threading.Tasks;
using LibrePay.Interfaces.Devices;
using LibrePay.Interfaces.Services.Navigation;
using LibrePay.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LibrePay.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaymentFinalizationPage
    {
        private readonly PaymentFinalizationPageViewModel _viewModel;
        private readonly INavigationService _navigationService;
        private readonly IMessageDisplayer _msgDisplayer;

        public PaymentFinalizationPage(
            PaymentFinalizationPageViewModel viewModel
            , INavigationService navigationService
            , IMessageDisplayer msgDisplayer
        )
        {
            InitializeComponent();

            _navigationService = navigationService;
            _msgDisplayer = msgDisplayer;

            BindingContext = _viewModel = viewModel;

            MessagingCenter.Subscribe<PaymentFinalizationPageViewModel, decimal>(
                _viewModel
                , MessengerKeys.PaymentFullyReceived
                , (_, value) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        _viewModel.HandleCompletePayment(value);

                        await _msgDisplayer.ShowMessageAsync($"Pagamento efetuado!\nPago: {value:N8}");
                        await ExitPageAsync();
                    });
                }
            );
            MessagingCenter.Subscribe<PaymentFinalizationPageViewModel, (decimal totalValue, decimal txValue)>(
                _viewModel
                , MessengerKeys.PaymentPartiallyReceived
                , (_, partialPayment) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        _viewModel.HandlePartialValuePayment(partialPayment);

                        await _msgDisplayer.ShowMessageAsync(
                            $"Valor recebido: {partialPayment.totalValue:N8}\n" +
                            $"Aguardando o restante ({_viewModel.MissingAmount:N8})..."
                        );
                    });
                }
            );
        }

        #region Overrides

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.CopyToClipboardAsync(_viewModel.Payment.Address)
                .ContinueWith(_ => _msgDisplayer.ShowMessageAsync("Copiado endereço"),
                    TaskContinuationOptions.OnlyOnRanToCompletion);

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
            await _viewModel.CopyToClipboardAsync(((Label) sender).Text);
            await _msgDisplayer.ShowMessageAsync("Copiado");
        }

        #endregion

        public async Task ExitPageAsync()
        {
            _viewModel.StopBackgroundJob();

            await _navigationService.PopModalStackAsync();

            _viewModel.NotifyMainPageOfPaymentFinalization();
        }
    }
}