using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Interfaces.Services;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.Services;
using NBitcoin;
using Xamarin.Forms;

[assembly: Dependency(typeof(PaymentService))]

namespace BitcoinPOS_App.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IBitcoinPriceProvider _btcPriceProvider;

        public PaymentService(
            ISettingsProvider settingsProvider
            , IBitcoinPriceProvider btcPriceProvider
        )
        {
            _settingsProvider = settingsProvider;
            _btcPriceProvider = btcPriceProvider;
        }

        public virtual async Task<Payment> GeneratePaymentAddressAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var rawXPub = await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsXPubKey);
            var bitcoinExtPubKey = new BitcoinExtPubKey(rawXPub);
            var xpub = bitcoinExtPubKey.ExtPubKey;

            var id = await _settingsProvider.GetValueAsync<long>(Constants.LastId) + 1L;
            await _settingsProvider.SetValueAsync(Constants.LastId, id);

            payment.Id = id;
            payment.Address = xpub.Derive((uint) id)
                .PubKey
                .GetAddress(bitcoinExtPubKey.Network)
                .ToString();
            payment.Done = false;

            return payment;
        }

        public virtual async Task<Payment> GenerateNewPayment(decimal valueFiat)
        {
            if (valueFiat <= 0)
                throw new ArgumentException("Invalid value", nameof(valueFiat));

            var payment = new Payment(valueFiat);

            await GeneratePaymentAddressAsync(payment);

            payment.ExchangeRate = await _btcPriceProvider.GetLocalBitcoinPrice();

            return payment;
        }
    }
}
