using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.ViewModels
{
    public class PaymentFinalizationViewModel : BaseViewModel
    {
        private Payment _payment;

        public Payment Payment
        {
            get => _payment;
            set => SetProperty(ref _payment, value);
        }
    }
}