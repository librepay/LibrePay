using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Interfaces.Services;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        private MainPageViewModel _viewModel;
        private readonly IMessageDisplayer _msgDisplayer;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IPaymentService _paymentService;
        private readonly IBitcoinPriceProvider _bitcoinPriceProvider;

        public Command CheckXPubKey { get; }

        public MainPage()
        {
            InitializeComponent();

            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();
            _settingsProvider = DependencyService.Get<ISettingsProvider>();
            _paymentService = DependencyService.Get<IPaymentService>();
            _bitcoinPriceProvider = DependencyService.Get<IBitcoinPriceProvider>();

            ResetViewModel();

            CheckXPubKey = new Command(async () =>
            {
                var xpubExists = await CheckXPubExists();

                if (xpubExists)
                    return;

                Debug.WriteLine("Pedindo para o usuário configurar a xpub key");

                Device.BeginInvokeOnMainThread(async () =>
                {
                    var alertResult = await DisplayAlert("Configurações",
                        "Para usar o aplicativo é necessário configurar uma Extended Public Key para poder gerar endereços de pagamento.",
                        "Ok", "Cancelar");

                    if (alertResult)
                        await Navigation.PushAsync(new SettingsPage());
                });
            });

            MessagingCenter.Subscribe<PaymentFinalizationPage>(this, "clean", _ => ResetViewModel());
        }

        #region Overrides

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(_ => CheckXPubKey.Execute(null));

            _bitcoinPriceProvider
                .GetLocalBitcoinPrice()
                .ContinueWith(t => _viewModel.BitcoinPrice = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        #endregion

        private async Task<bool> CheckXPubExists()
        {
            Debug.WriteLine("Checkando se tem xpub key configurada...");
            var xpub = await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsXPubKey);
            return !string.IsNullOrWhiteSpace(xpub);
        }

        private void ResetViewModel()
        {
            BindingContext = _viewModel = new MainPageViewModel
            {
                // maintains this value 
                BitcoinPrice = _viewModel?.BitcoinPrice
            };
        }

        #region Events

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void Number_Clicked(object sender, EventArgs e)
        {
            var bt = sender as Button;

            if (bt == null)
                return;

            Debug.WriteLine($"Botão apertado: {bt.AutomationId}");

            if (bt.AutomationId == "virgula" && _viewModel.TransactionValueStr?.Contains(",") == true)
                return;

            _viewModel.TransactionValueStr = _viewModel.TransactionValueStr + bt.Text;

            // verifies if there's more than 3 digits after the decimal separator
            // and if there's any shows a toast
            var values = _viewModel.TransactionValueStr.Split(',');
            if (values.Length > 1 && values[1].Length == 3)
            {
                Debug.WriteLine("Mostrou msg de valor arredondado");
                await _msgDisplayer.ShowMessageAsync("O valor será arredondado!");
            }

            Debug.WriteLine($"Novo valor: {_viewModel.TransactionValueStr}");
        }

        private async void Pay_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine($"Pagar pressionado: {_viewModel.TransactionValue}");

            if (_viewModel.TransactionValue == 0 || _viewModel.IsBusy)
                return;

            Debug.WriteLine("Seguindo com o pagamento");

            var xpubExists = await CheckXPubExists();

            if (!xpubExists)
            {
                await _msgDisplayer.ShowMessageAsync("Configure a sua Extended Public Key antes...");
                return;
            }

            _viewModel.IsBusy = true;
            Debug.WriteLine("Seguindo com o pagamento");

            try
            {
                Debug.WriteLine("Gerando pagamento...");

                var payment = await _paymentService.GenerateNewPayment(_viewModel);

                await Navigation.PushModalAsync(
                    new PaymentFinalizationPage(
                        new PaymentFinalizationViewModel
                        {
                            Payment = payment
                        }
                    )
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await _msgDisplayer.ShowMessageAsync("Aconteceu um erro.");
            }
            finally
            {
                _viewModel.IsBusy = false;
            }
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Limpar pressionado");
            ResetViewModel();
        }

        #endregion
    }
}