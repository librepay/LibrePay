using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces;
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

        public PaymentService()
        {
            _settingsProvider = DependencyService.Get<ISettingsProvider>();
        }


        public async Task<Payment> GeneratePaymentAddressAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            var rawXPub = await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsXPubKey);
            var xpub = new BitcoinExtPubKey(rawXPub, expectedNetwork: Constants.NetworkInUse).ExtPubKey;

            var id = await _settingsProvider.GetValueAsync<long>(Constants.LastId) + 1L;
            await _settingsProvider.SetValueAsync(Constants.LastId, id);

            payment.Id = id;
            payment.Address = xpub.Derive((uint) id)
                .PubKey
                .GetAddress(Constants.NetworkInUse)
                .ToString();
            payment.Done = false;

            return payment;
        }
    }
}