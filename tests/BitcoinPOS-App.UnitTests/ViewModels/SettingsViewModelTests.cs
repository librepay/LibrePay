using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.ViewModels;
using Moq;
using Xamarin.Forms;
using Xunit;

namespace BitcoinPOS_App.UnitTests.ViewModels
{
    public class SettingsViewModelTests
    {
        private static SettingsPageViewModel Get(
            out Mock<ISettingsProvider> mockSettings
        )
        {
            mockSettings = new Mock<ISettingsProvider>(MockBehavior.Strict);
            return new SettingsPageViewModel(
                mockSettings.Object
            );
        }

        [Fact]
        public async Task LoadSettingsAsyncCallsSettingProviderAsync()
        {
            var vm = Get(out var mockSettings);
            mockSettings.Setup(s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == Constants.SettingsXPubKey)))
                .Returns(Task.FromResult("success"));

            await vm.LoadSettingsAsync();

            mockSettings.Verify(
                s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == Constants.SettingsXPubKey))
                , Times.Once
            );
        }

        [Fact]
        public async Task LoadSettingsAsyncShouldSendAMessageWhenLoadingFailed()
        {
            var vm = Get(out var mockSettings);

            var exception = new Exception("hey! just testing");
            mockSettings.Setup(s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == Constants.SettingsXPubKey)))
                .Returns(Task.FromException<string>(exception));

            var messageReceived = false;

            MessagingCenter.Subscribe<SettingsPageViewModel, Exception>(
                this
                , MessengerKeys.SettingsFailedLoadSettings
                , (_, arg) =>
                {
                    messageReceived = true;
                    Assert.Same(exception, arg.InnerException);
                });
            await vm.LoadSettingsAsync();

            Assert.True(messageReceived);

            mockSettings.Verify(
                s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == Constants.SettingsXPubKey))
                , Times.Once
            );
        }

        [Fact]
        public async Task SaveSettingsAsyncCallsSettingsProviderAsync()
        {
            var vm = Get(out var mockSettings);
            mockSettings.Setup(
                s => s.SetSecureValueAsync(It.Is<string>(i => i == Constants.SettingsXPubKey), It.IsAny<string>())
            ).Returns(Task.CompletedTask);
            mockSettings.Setup(
                s => s.SetValueAsync(It.Is<string>(i => i == Constants.LastId), It.IsAny<long>())
            ).Returns(Task.CompletedTask);

            await vm.SaveSettingsAsync();

            mockSettings.Verify(
                s => s.SetSecureValueAsync(It.Is<string>(i => i == Constants.SettingsXPubKey), It.IsAny<string>())
                , Times.Once
            );
            mockSettings.Verify(
                s => s.SetValueAsync(It.Is<string>(i => i == Constants.LastId), It.IsAny<long>())
                , Times.Once
            );
        }
    }
}
