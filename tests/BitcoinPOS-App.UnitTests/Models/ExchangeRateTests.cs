using System;
using System.Globalization;
using System.Linq;
using BitcoinPOS_App.Models;
using Xunit;

namespace BitcoinPOS_App.UnitTests.Models
{
    public class ExchangeRateTests
    {
        [Fact]
        public void ToStringIsOverriden()
        {
            var er = new ExchangeRate
            {
                Date = DateTime.Now,
                Rate = 0.5M
            };

            Assert.Equal($"Rate: {er.DisplayRate}\n" + $"Date: {er.Date:d}", er.ToString());
        }

        [Fact]
        public void GetExchangedValueReturnsANumberRoundedBy8()
        {
            var er = new ExchangeRate
            {
                Date = DateTime.Now,
                Rate = 5999999M
            };
            var valueFiat = 15M;

            var result = er.GetExchangedValue(valueFiat);

            var numberOfDecimals = result.ToString(CultureInfo.InvariantCulture).Split('.').Last().Length;
            Assert.Equal(numberOfDecimals, Constants.BitcoinDecimals);

            var roundValue = Math.Round(valueFiat / er.Rate, Constants.BitcoinDecimals, MidpointRounding.AwayFromZero);
            Assert.Equal(result, roundValue);
        }
    }
}
