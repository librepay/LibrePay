using System.Diagnostics;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BitcoinPOS_App.ViewModels
{
    public class PaymentFinalizationViewModel : BaseViewModel
    {
        private Payment _payment = new Payment();
        private readonly INetworkInfoProvider _netInfoProvider;
        private BackgroundJob _backgroundJob;

        public Payment Payment
        {
            get => _payment;
            set => SetProperty(ref _payment, value);
        }

        public PaymentFinalizationViewModel(INetworkInfoProvider netInfoProvider)
        {
            _netInfoProvider = netInfoProvider;
        }

        public virtual void AcceptPayment(decimal value)
        {
            Payment.Done = true;

            Device.BeginInvokeOnMainThread(() =>
                MessagingCenter.Send(this, MessengerKeys.PaymentFullyReceived, value)
            );
        }

        public virtual void CopyToClipboard(string value)
        {
            Debug.WriteLine("Copiando valor: {0}", (object) value);
            Clipboard.SetText(value);
        }

        public virtual void StartBackgroundJob()
        {
            if (string.IsNullOrWhiteSpace(Payment.Address))
                return;

            StopBackgroundJob();

            _backgroundJob = _netInfoProvider.WaitCompletePayment(
                Payment
                , AcceptPayment
                , (totalValue, txValue) =>
                {
                    Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(
                        this
                        , MessengerKeys.PaymentPartiallyReceived
                        , (totalValue, txValue)
                    ));
                });
        }

        public virtual void StopBackgroundJob()
        {
            _backgroundJob?.Cancel();
        }

        public virtual void NotifyMainPageOfPaymentFinalization()
        {
            MessagingCenter.Send(this, MessengerKeys.MainFinishPayment);
        }
    }
}
