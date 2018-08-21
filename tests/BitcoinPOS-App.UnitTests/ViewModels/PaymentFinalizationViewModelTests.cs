using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.UnitTests.TestUtility;
using BitcoinPOS_App.ViewModels;
using Moq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xunit;

namespace BitcoinPOS_App.UnitTests.ViewModels
{
    public class PaymentFinalizationViewModelTests
    {
        private PaymentFinalizationViewModel Get(
            out Mock<INetworkInfoProvider> mockNet
        )
        {
            mockNet = new Mock<INetworkInfoProvider>(MockBehavior.Strict);
            return new PaymentFinalizationViewModel(
                mockNet.Object
            );
        }

        [Fact]
        public void AcceptPaymentSendAMessageWhenDone()
        {
            var vm = Get(out _);
            const decimal value = 5.5M;
            var messageReceived = false;

            MessagingCenter.Subscribe<PaymentFinalizationViewModel, decimal>(
                this
                , MessengerKeys.PaymentFullyReceived
                , (_, arg) =>
                {
                    messageReceived = true;
                    Assert.Equal(value, arg);
                });

            vm.AcceptPayment(value);

            Assert.True(vm.Payment.Done);
            Assert.True(messageReceived);
        }

        [FactOnlyInMobile]
        [Trait("Category", "Mobile")]
        public async Task CopyToClipboardShouldWorkAsync()
        {
            var vm = Get(out _);

            vm.CopyToClipboard("test");

            Assert.Equal("test", await Clipboard.GetTextAsync());
        }

        [Fact]
        public void StartBackgroundJobChecksIfPaymentAddressIsNotEmpty()
        {
            var mockNet = new Mock<INetworkInfoProvider>(MockBehavior.Strict);
            var mock = new Mock<PaymentFinalizationViewModel>(
                mockNet.Object
            )
            {
                CallBase = true
            };
            var vm = mock.Object;

            vm.Payment = new Payment
            {
                Address = null
            };

            vm.StartBackgroundJob();

            mock.Verify(m => m.StopBackgroundJob(), Times.Never);
        }

        [Fact]
        public void StartBackgroundJobShouldStopOtherIfRunning()
        {
            var mockNet = new Mock<INetworkInfoProvider>(MockBehavior.Strict);
            var mock = new Mock<PaymentFinalizationViewModel>(
                mockNet.Object
            )
            {
                CallBase = true
            };
            var vm = mock.Object;
            mockNet.Setup(m =>
                m.WaitCompletePayment(
                    It.Is<Payment>(i => i.Address == vm.Payment.Address)
                    , It.IsAny<Action<decimal>>()
                    , It.IsAny<Action<decimal, decimal>>()
                )
            ).Returns(new BackgroundJob(null));

            vm.Payment = new Payment
            {
                Address = FakeData.TestnetBtcAddress
            };

            vm.StartBackgroundJob();

            mock.Verify(m => m.StopBackgroundJob(), Times.Once);
            mockNet.Verify(m =>
                m.WaitCompletePayment(
                    It.Is<Payment>(i => i.Address == vm.Payment.Address)
                    , It.IsAny<Action<decimal>>()
                    , It.IsAny<Action<decimal, decimal>>()
                )
                , Times.Once
            );
        }
    }
}
