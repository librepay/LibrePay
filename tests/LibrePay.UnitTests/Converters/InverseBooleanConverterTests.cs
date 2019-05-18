using System;
using LibrePay.Converters;
using Xunit;

namespace LibrePay.UnitTests.Converters
{
    public class InverseBooleanConverterTests
    {
        [Fact]
        public void ShouldInverseABool()
        {
            var ibc = new InverseBooleanConverter();

            var result = ibc.Convert(false, typeof(bool), null, null);

            Assert.IsType<bool>(result);
            Assert.True((bool) result);

            result = ibc.ConvertBack(result, typeof(bool), null, null);

            Assert.IsType<bool>(result);
            Assert.False((bool) result);
        }

        [Fact]
        public void ShouldThrowIfTargetIsNotABool()
        {
            var ibc = new InverseBooleanConverter();

            Assert.Throws<InvalidOperationException>(() => ibc.Convert(false, null, null, null));
            Assert.Throws<InvalidOperationException>(() => ibc.ConvertBack(false, null, null, null));
            Assert.Throws<InvalidOperationException>(() => ibc.Convert(false, typeof(string), null, null));
            Assert.Throws<InvalidOperationException>(() => ibc.ConvertBack(false, typeof(string), null, null));
        }
    }
}
