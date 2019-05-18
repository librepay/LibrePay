using System;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.Interfaces.Services;
using LibrePay.Models;
using LibrePay.Services;
using NBitcoin;
using Xamarin.Forms;

[assembly: Dependency(typeof(PaymentService))]

namespace LibrePay.Services
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
            var useSegwit = await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsUseSegwit);

            var bitcoinExtPubKey = new BitcoinExtPubKey(rawXPub);
            var xpub = bitcoinExtPubKey.ExtPubKey;

            var id = await _settingsProvider.GetValueAsync<long>(Constants.LastId) + 1L;
            await _settingsProvider.SetValueAsync(Constants.LastId, id);

            payment.Id = id;

            // !!! Derivation path changed to be compatible with Coinomi wallet
            // !!! This path must be set in the Settings page
            KeyPath path = new KeyPath("0/" + id);

            if (useSegwit.ToUpper() == "YES")
            {
                payment.Address = xpub.Derive(path)
                    .PubKey
                    .GetAddress(ScriptPubKeyType.Segwit, bitcoinExtPubKey.Network)
                    .ToString();
            }
            else
            {
                payment.Address = xpub.Derive(path)
                    .PubKey
                    .GetAddress(ScriptPubKeyType.SegwitP2SH, bitcoinExtPubKey.Network)
                    .ToString();
            }

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