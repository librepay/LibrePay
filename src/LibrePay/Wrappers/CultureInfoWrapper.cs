using System;
using System.Globalization;

namespace LibrePay.Wrappers
{
    public class CultureInfoWrapper
    {
        public CultureInfo CultureInfo { get; }

        public CultureInfoWrapper(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
        }

        public override string ToString()
            => $"{CultureInfo.NativeName} - {CultureInfo.IetfLanguageTag}";
    }
}
