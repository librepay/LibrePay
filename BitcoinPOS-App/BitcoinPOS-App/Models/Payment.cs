using System;
using System.Diagnostics;
using BitcoinPOS_App.ViewModels;

namespace BitcoinPOS_App.Models
{
    [DebuggerDisplay("#{Id} - {Address} - {Value:n2}", Name = "#{Id}")]
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
            Value = Math.Round(viewModel.TransactionValue, 2, MidpointRounding.AwayFromZero);
            Debug.WriteLine($"Valor arredondado de {viewModel.TransactionValue} => {Value}");
        }
    }
}