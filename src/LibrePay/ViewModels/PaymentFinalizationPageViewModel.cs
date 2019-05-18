using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.Models;
using LibrePay.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LibrePay.ViewModels
{
    public class PaymentFinalizationPageViewModel : BaseViewModel
    {
        private Payment _payment = new Payment();
        private decimal _missingAmount;
        private readonly INetworkInfoProvider _netInfoProvider;
        private BackgroundJob _backgroundJob;

        public Payment Payment
        {
            get => _payment;
            set => SetProperty(ref _payment, value);
        }

        public decimal MissingAmount
        {
            get => _missingAmount;
            set => SetProperty(ref _missingAmount, value);
        }

        public PaymentFinalizationPageViewModel(INetworkInfoProvider netInfoProvider)
        {
            _netInfoProvider = netInfoProvider;
            PropertyChanged += PaymentFinalizationViewModel_PropertyChanged;
        }

        public override Task InitializeAsync(object[] navigationData)
        {
            Payment = (Payment)navigationData[0];

            return base.InitializeAsync(navigationData);
        }

        private void PaymentFinalizationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Payment))
            {
                MissingAmount = Payment.ValueBitcoin;
            }
        }

        public virtual void AcceptPayment(decimal value)
        {
            Payment.Done = true;

            RunOnMainThread(() =>
                MessagingCenter.Send(this, MessengerKeys.PaymentFullyReceived, value)
            );
        }

        public virtual void CopyToClipboard(string value)
        {
            Debug.WriteLine("Copiando valor: {0}", (object)value);
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
                    RunOnMainThread(() => MessagingCenter.Send(
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

        public virtual void HandleCompletePayment(decimal value)
        {
            Debug.WriteLine("Payment finished");
        }

        public virtual void HandlePartialValuePayment((decimal totalValue, decimal txValue) partialPayment)
        {
            MissingAmount = MissingAmount - partialPayment.txValue;
        }
    }
}
