using System.ComponentModel;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private decimal _transactionValue = decimal.Zero;
        private string _transactionValueStr;
        private bool _isBusy;
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
            set => SetProperty(ref _isBusy, value);
        }

        public ExchangeRate BitcoinPrice
        {
            get => _bitcoinPrice;
            set => SetProperty(ref _bitcoinPrice, value);
        }

        public MainPageViewModel()
        {
            PropertyChanged += MainPageViewModel_PropertyChanged;
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