using System.Diagnostics.CodeAnalysis;
using BitcoinPOS_App.Models;
using Xunit;

namespace BitcoinPOS_App.UnitTests.Models
{
    public class TransactionTests
    {
        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
        public void TransactionChecksEquityById()
        {
            Assert.True(new Transaction("1").Equals(new Transaction("1")));
            Assert.False(new Transaction("1").Equals(null));
            Assert.False(new Transaction("1").Equals("1"));
            Assert.True(new Transaction("1") == new Transaction("1"));
            Assert.False(new Transaction("1") == new Transaction("2"));
            Assert.False(new Transaction("1") == null);
            Assert.True(new Transaction("1") != new Transaction("2"));
            Assert.True(new Transaction("1") != null);
        }

        [Fact]
        public void TransactionUseIdHashCode()
        {
            Assert.Equal(new Transaction("1") {Confirmations = 6}.GetHashCode(), new Transaction("1").GetHashCode());
        }
    }
}
