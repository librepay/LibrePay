using System;

namespace BitcoinPOS_App.Models
{
    public class ExchangeRate
    {
        public decimal Rate { get; set; }

        public ExchangeSymbol Symbol { get; set; }

        public string DisplayRate => $"{Symbol} {Rate:N2}";

        public DateTime Date { get; set; }

        public decimal ExchangeValueTo(decimal valueFrom, int decimals = Constants.BitcoinDecimals)
            => Math.Round(valueFrom / Rate, decimals, MidpointRounding.AwayFromZero);

        public decimal ExchangeValueFrom(decimal valueTo, int decimals = Constants.FiatDecimals)
            => Math.Round(valueTo * Rate, decimals, MidpointRounding.AwayFromZero);

        public ExchangeRate(decimal rate, string fromToSymbol, DateTime date)
        {
            Rate = rate;
            Date = date;
            Symbol = new ExchangeSymbol(fromToSymbol);
        }

        public ExchangeRate(decimal rate, string fromSymbol, string toSymbol, DateTime date)
        {
            Rate = rate;
            Date = date;
            Symbol = new ExchangeSymbol(fromSymbol, toSymbol);
        }

        public ExchangeRate(decimal rate, ExchangeSymbol exchangeSymbol, DateTime date)
        {
            Rate = rate;
            Date = date;
            Symbol = exchangeSymbol ?? throw new ArgumentNullException(nameof(exchangeSymbol));
        }

        public override string ToString()
        {
            return $"Rate: {DisplayRate}\n" +
                   $"Date: {Date:d}";
        }
    }
}
