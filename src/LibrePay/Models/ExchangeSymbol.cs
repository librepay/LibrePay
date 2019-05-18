using System;
using System.Diagnostics;

namespace LibrePay.Models
{
    [DebuggerDisplay("{From}{Separator}{To}")]
    public class ExchangeSymbol
    {
        public string From { get; set; }

        public string To { get; set; }

        public char Separator { get; set; }

        public ExchangeSymbol(string from, string to)
        {
            From = from;
            To = to;
        }

        public ExchangeSymbol(string fromTo)
        {
            InternalParse(fromTo);
        }

        private void InternalParse(string fromTo)
        {
            if (fromTo == null)
                throw new ArgumentNullException(nameof(fromTo));

            var s = fromTo.Split(Separator = '/');

            bool TryThis()
            {
                if (s.Length != 2)
                    return false;

                From = s[0].Trim();
                To = s[1].Trim();

                return true;
            }

            if (TryThis())
                return;

            s = fromTo.Split(Separator = '\\');

            if (TryThis())
                return;

            s = fromTo.Split(Separator = '-');

            if (!TryThis())
                throw new ArgumentException("Invalid exchange symbol pair", nameof(fromTo));
        }

        public static ExchangeSymbol Parse(string exchangeSymbol)
            => new ExchangeSymbol(exchangeSymbol);

        public override string ToString()
            => $"{From}{Separator}{To}";
    }
}
