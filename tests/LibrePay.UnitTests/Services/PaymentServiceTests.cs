using System;
using System.Globalization;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.Interfaces.Services;
using LibrePay.Models;
using LibrePay.Services;
using Moq;
using Xunit;

namespace LibrePay.UnitTests.Services
{
    public class PaymentServiceTests
    {
        private static IPaymentService Get(
            out Mock<PaymentService> mockPayment
            , out Mock<ISettingsProvider> mockSettings
            , out Mock<IBitcoinPriceProvider> mockBtcProvider
        )
        {
            mockSettings = new Mock<ISettingsProvider>(MockBehavior.Strict);
            mockBtcProvider = new Mock<IBitcoinPriceProvider>(MockBehavior.Strict);
            mockPayment = new Mock<PaymentService>(
                MockBehavior.Strict
                , mockSettings.Object
                , mockBtcProvider.Object
            );
            return mockPayment.Object;
        }

        [Fact]
        public async Task ShouldValidateValueWhenGeneratingANewPaymentAsync()
        {
            var service = Get(
                out var mockPayment
                , out _
                , out _
            );
            mockPayment.Setup(p => p.GenerateNewPayment(It.IsAny<decimal>()))
                .CallBase();

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.GenerateNewPayment(0));
            Assert.StartsWith("Invalid value", ex.Message);
        }

        [Fact]
        public async Task CanGenerateACompletePayment()
        {
            var service = Get(
                out var mockPayment
                , out var mockSettings
                , out var mockBtcProvider
            );
            mockPayment.Setup(p => p.GenerateNewPayment(It.IsAny<decimal>()))
                .CallBase();
            mockPayment.Setup(p => p.GeneratePaymentAddressAsync(It.IsAny<Payment>()))
                .CallBase();

            mockSettings.Setup(s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == SettingsKeys.XPubKey)))
                .ReturnsAsync(FakeData.ValidXPub);
            mockSettings.Setup(s => s.GetValueAsync<bool>(It.Is<string>(i => i == SettingsKeys.UseSegwit)))
                .ReturnsAsync(true);
            mockSettings.Setup(s => s.GetValueAsync<long>(It.Is<string>(i => i == SettingsKeys.LastId)))
                .ReturnsAsync(1L);
            mockSettings.Setup(s =>
                    s.SetValueAsync(It.Is<string>(i => i == SettingsKeys.LastId), It.Is<long>(i => i == 2L)))
                .Returns(Task.CompletedTask);

            var exchangeRate = new ExchangeRate(0.5M, "R$/BTC", DateTime.Now, CultureInfo.InvariantCulture);
            mockBtcProvider.Setup(b => b.GetLocalBitcoinPrice())
                .Returns(Task.FromResult(exchangeRate));

            var result = await service.GenerateNewPayment(50M);

            Assert.NotNull(result);
            Assert.Equal(FakeData.ValidXPubAddressDerivedNum2, result.Address);
            Assert.False(result.Done);
            Assert.Equal(100M, result.ValueBitcoin);
            Assert.Equal(2L, result.Id);
            Assert.Same(exchangeRate, result.ExchangeRate);

            // verifications
            mockPayment.Verify(p => p.GeneratePaymentAddressAsync(It.IsAny<Payment>()), Times.Once);
            mockSettings.Verify(s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == SettingsKeys.XPubKey))
                , Times.Once);
            mockSettings.Verify(s => s.GetValueAsync<bool>(It.Is<string>(i => i == SettingsKeys.UseSegwit)), Times.Once);
            mockSettings.Verify(s => s.GetValueAsync<long>(It.Is<string>(i => i == SettingsKeys.LastId)), Times.Once);
            mockSettings.Verify(s =>
                s.SetValueAsync(It.Is<string>(i => i == SettingsKeys.LastId), It.Is<long>(i => i == 2L)), Times.Once);
            mockBtcProvider.Verify(b => b.GetLocalBitcoinPrice(), Times.Once);
        }
    }
}
