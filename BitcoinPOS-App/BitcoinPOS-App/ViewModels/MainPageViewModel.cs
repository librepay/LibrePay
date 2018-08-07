using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Interfaces.Services;
using BitcoinPOS_App.Models;
using Xamarin.Forms;

namespace BitcoinPOS_App.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly IMessageDisplayer _msgDisplayer;
        private readonly IPaymentService _paymentService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IBitcoinPriceProvider _bitcoinPriceProvider;

        private decimal _transactionValue = decimal.Zero;
        private string _transactionValueStr;
        private bool _isBusy = true;
        private ExchangeRate _bitcoinPrice;

        public decimal TransactionValue
        {
            get => _transactionValue;
            set => SetProperty(ref _transactionValue, value);
        }

        public string TransactionValueStr
        {
            get => _transactionValueStr;
            set => SetProperty(ref _transactionValueStr, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public ExchangeRate BitcoinPrice
        {
            get => _bitcoinPrice;
            set => SetProperty(ref _bitcoinPrice, value);
        }

        public Command PinpadNumberCommand { get; }

        public MainPageViewModel(
            IMessageDisplayer msgDisplayer
            , IPaymentService paymentService
            , ISettingsProvider settingsProvider
            , IBitcoinPriceProvider bitcoinPriceProvider
        )
        {
            PropertyChanged += MainPageViewModel_PropertyChanged;
            PinpadNumberCommand = new Command(SetPinpadNumber);

            _msgDisplayer = msgDisplayer;
            _paymentService = paymentService;
            _settingsProvider = settingsProvider;
            _bitcoinPriceProvider = bitcoinPriceProvider;

            MessagingCenter.Subscribe<PaymentFinalizationViewModel>(
                this
                , MessengerKeys.MainFinishPayment
                , _ => ResetPinpad()
            );
        }

        public async Task InitializeAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(async _ =>
                {
                    var xpubExists = await CheckXPubExists();

                    // if the xpub doesn't exists then
                    // doesn't allow to press any buttons
                    IsBusy = !xpubExists;

                    MessagingCenter.Send(this
                        , MessengerKeys.MainCheckXPubExistence
                        , xpubExists
                    );
                })
                .ConfigureAwait(false);

            await _bitcoinPriceProvider
                .GetLocalBitcoinPrice()
                .ContinueWith(t => BitcoinPrice = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ConfigureAwait(false);
        }

        public async Task<bool> CheckXPubExists()
        {
            Debug.WriteLine("Checkando se tem xpub key configurada...");
            var xpub = await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsXPubKey)
                .ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(xpub);
        }

        public void ResetPinpad()
        {
            Debug.WriteLine("Reiniciando o pinpad...");
            TransactionValueStr = "0";
            IsBusy = false;
        }

        public async Task<Payment> GenerateNewPayment()
        {
            if (TransactionValue == 0 || IsBusy)
                return null;

            IsBusy = true;
            Debug.WriteLine("Seguindo com o pagamento");

            try
            {
                var payment = await _paymentService.GenerateNewPayment(this)
                    .ConfigureAwait(false);
                Debug.WriteLine("Pagamento gerado: {0}", payment);

                return payment;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void SetPinpadNumber(object obj)
        {
            var value = obj as string;
            if (string.IsNullOrWhiteSpace(value))
                return;

            Debug.WriteLine($"Botão apertado: {value}");

            if (value == "virgula" && TransactionValueStr?.Contains(",") == true)
                return;

            TransactionValueStr = TransactionValueStr + value;

            // verifies if there's more than 3 digits after the decimal separator
            // and if there's any shows a toast
            var values = TransactionValueStr.Split(',');
            if (values.Length > 1 && values[1].Length == 3)
            {
                Debug.WriteLine("Mostrou msg de valor arredondado");
                await _msgDisplayer.ShowMessageAsync("O valor será arredondado!");
            }

            Debug.WriteLine($"Novo valor: {TransactionValueStr}");
        }

        private void MainPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TransactionValueStr))
            {
                decimal.TryParse(TransactionValueStr, out var tranValue);
                TransactionValue = tranValue;
            }
        }
    }
}