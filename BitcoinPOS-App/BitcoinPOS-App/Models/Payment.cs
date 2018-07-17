using System;
using BitcoinPOS_App.ViewModels;

namespace BitcoinPOS_App.Models
{
    public class Payment
    {
        private decimal _value;

        public bool Done { get; set; }

        public long Id { get; set; }

        public decimal Value
        {
            get => _value;
            set
            {
                if (!string.IsNullOrWhiteSpace(Address))
                    throw new InvalidOperationException("Não é possível alterar o valor após iniciado o pagamento");

                _value = value;
            }
        }

        public string Address { get; set; }

        public Payment()
        {
        }

        public Payment(MainPageViewModel viewModel)
        {
            Value = viewModel.TransactionValue;
        }
    }
}