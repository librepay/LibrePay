using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.Interfaces.Services;
using LibrePay.Models;
using LibrePay.ViewModels.Base;
using Xamarin.Forms;

namespace LibrePay.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IBitcoinPriceProvider _bitcoinPriceProvider;
        private readonly CultureInfo _cultureInfo;

        private Pinpad _transactionValueCrypto;
        private Pinpad _transactionValue;
        private bool _isEnteringCryptoValue;
        private bool _isBusy = true;
        private ExchangeRate _exchangeRate;

        public Pinpad TransactionValueCrypto
        {
            get => _transactionValueCrypto;
            set => SetProperty(ref _transactionValueCrypto, value);
        }

        public Pinpad TransactionValue
        {
            get => _transactionValue;
            set => SetProperty(ref _transactionValue, value);
        }

        public bool IsEnteringCryptoValue
        {
            get => _isEnteringCryptoValue;
            private set => SetProperty(ref _isEnteringCryptoValue, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public ExchangeRate ExchangeRate
        {
            get => _exchangeRate;
            set => SetProperty(ref _exchangeRate, value);
        }

        public Command PinpadNumberCommand { get; }

        public MainPageViewModel(
            IPaymentService paymentService
            , ISettingsProvider settingsProvider
            , IBitcoinPriceProvider bitcoinPriceProvider
            , CultureInfo cultureInfo
        )
        {
            PinpadNumberCommand = new Command(SetPinpadNumber);

            _paymentService = paymentService;
            _settingsProvider = settingsProvider;
            _bitcoinPriceProvider = bitcoinPriceProvider;

            _cultureInfo = cultureInfo;

            ResetPinpad();

            MessagingCenter.Subscribe<PaymentFinalizationPageViewModel>(
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
                .ContinueWith(t => ExchangeRate = t.Result, TaskContinuationOptions.OnlyOnRanToCompletion)
                .ConfigureAwait(false);
        }

        public async Task<bool> CheckXPubExists()
        {
            Debug.WriteLine("Checkando se tem xpub key configurada...");
            var xpub = await _settingsProvider.GetSecureValueAsync<string>(SettingsKeys.XPubKey)
                .ConfigureAwait(false);
            return !string.IsNullOrWhiteSpace(xpub);
        }

        public void ResetPinpad()
        {
            Debug.WriteLine("Reiniciando o pinpad...", "INFO");

            TransactionValue = new Pinpad(2, 10, _cultureInfo);
            TransactionValueCrypto = new Pinpad(8, 11, _cultureInfo);

            IsBusy = false;
        }

        public void BackspacePress()
        {
            Debug.WriteLine("Backspace", "PINPAD");

            if (IsEnteringCryptoValue)
            {
                TransactionValueCrypto.DeleteLastNumber();
                UpdateExchangedValueFiat();
            }
            else
            {
                TransactionValue.DeleteLastNumber();
                UpdateExchangedValueCrypto();
            }

            Debug.WriteLine($"Novo valor: {TransactionValueCrypto.Value}", "INFO - CRYPTO");
            Debug.WriteLine($"Novo valor: {TransactionValue.Value}", "INFO - FIAT");
        }

        public async Task<Payment> GenerateNewPayment()
        {
            if (TransactionValue.ValueDecimal == 0 || IsBusy)
                return null;

            IsBusy = true;
            Debug.WriteLine("Seguindo com o pagamento");

            var payment = await _paymentService.GenerateNewPayment(TransactionValue.ValueDecimal)
                .ConfigureAwait(false);
            Debug.WriteLine("Pagamento gerado: {0}", payment);

            return payment;
        }

        private void SetPinpadNumber(object obj)
        {
            if (obj == null)
                return;

            var key = ((string) obj)[0];

            Debug.WriteLine(key.ToString(), "PINPAD");

            if (IsEnteringCryptoValue)
            {
                AppendKeyToCryptoValue(key);
                UpdateExchangedValueFiat();
            }
            else
            {
                AppendKeyToFiatValue(key);
                UpdateExchangedValueCrypto();
            }

            Debug.WriteLine($"Novo valor: {TransactionValueCrypto.Value}", "INFO - CRYPTO");
            Debug.WriteLine($"Novo valor: {TransactionValue.Value}", "INFO - FIAT");
        }

        private void AppendKeyToFiatValue(char key)
        {
            TransactionValue.AppendNumber(key);
        }

        private void AppendKeyToCryptoValue(char key)
        {
            TransactionValueCrypto.AppendNumber(key);
        }

        private void UpdateExchangedValueFiat()
        {
            TransactionValue.Value = ExchangeRate?.ExchangeValueFrom(TransactionValueCrypto.ValueDecimal)
                .ToString(TransactionValue.Format, _cultureInfo);
        }

        private void UpdateExchangedValueCrypto()
        {
            TransactionValueCrypto.Value = ExchangeRate?.ExchangeValueTo(TransactionValue.ValueDecimal)
                .ToString(TransactionValueCrypto.Format, _cultureInfo);
        }

        public bool SwitchEnteringSymbol()
        {
            IsEnteringCryptoValue = !IsEnteringCryptoValue;
            Debug.WriteLine($"Change input type to {(IsEnteringCryptoValue ? "crypto" : "fiat")}", "PINPAD");

            return IsEnteringCryptoValue;
        }
    }
}
