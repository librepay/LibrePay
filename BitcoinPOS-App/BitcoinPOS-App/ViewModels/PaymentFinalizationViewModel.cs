using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.ViewModels
{
    public class PaymentFinalizationViewModel : BaseViewModel
    {
        private Payment _payment = new Payment();
        private string _actionText = string.Empty;

        public Payment Payment
        {
            get => _payment;
            set => SetProperty(ref _payment, value);
        }

        public string ActionText
        {
            get => _actionText;
            set => SetProperty(ref _actionText, value);
        }
    }
}