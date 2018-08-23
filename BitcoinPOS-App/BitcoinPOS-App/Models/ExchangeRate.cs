using System;

namespace BitcoinPOS_App.Models
{
    public class ExchangeRate
    {
        public decimal Rate { get; set; }

        public ExchangeSymbol Symbol { get; set; } = new ExchangeSymbol("R$/BTC");

        public string DisplayRate => $"{Symbol} {Rate:N2}";

        public DateTime Date { get; set; }

        public decimal GetExchangedValue(decimal valueFiat)
            => Math.Round(valueFiat / Rate, Constants.BitcoinDecimals, MidpointRounding.AwayFromZero);

        [Obsolete("Specify exchange pair symbols", error: false)]
        public ExchangeRate()
        {
            Date = DateTime.Now;
        }

        [Obsolete("Specify exchange pair symbols", error: false)]
        public ExchangeRate(decimal rate, DateTime date)
        {
            Rate = rate;
            Date = date;
        }

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
