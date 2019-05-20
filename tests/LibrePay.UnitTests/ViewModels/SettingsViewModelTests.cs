using System;
using System.Globalization;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.ViewModels;
using Moq;
using Xamarin.Forms;
using Xunit;

namespace LibrePay.UnitTests.ViewModels
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
                , CultureInfo.DefaultThreadCurrentCulture
            );
        }

        [Fact]
        public async Task LoadSettingsAsyncCallsSettingProviderAsync()
        {
            var vm = Get(out var mockSettings);
            mockSettings.Setup(s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == SettingsKeys.XPubKey)))
                .ReturnsAsync("success");
            mockSettings.Setup(s => s.GetValueAsync<bool>(It.Is<string>(i => i == SettingsKeys.UseSegwit)))
                .ReturnsAsync(true);

            await vm.LoadSettingsAsync();

            mockSettings.Verify(
                s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == SettingsKeys.XPubKey))
                , Times.Once
            );
            mockSettings.Verify(s => s.GetValueAsync<bool>(It.Is<string>(i => i == SettingsKeys.UseSegwit))
                , Times.Once
            );
        }

        [Fact]
        public async Task LoadSettingsAsyncShouldSendAMessageWhenLoadingFailed()
        {
            var vm = Get(out var mockSettings);

            var exception = new Exception("hey! just testing");
            mockSettings.Setup(s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == SettingsKeys.XPubKey)))
                .Returns(Task.FromException<string>(exception));
            mockSettings.Setup(s => s.GetValueAsync<bool>(It.Is<string>(i => i == SettingsKeys.UseSegwit)))
                .ReturnsAsync(true);

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
                s => s.GetSecureValueAsync<string>(It.Is<string>(i => i == SettingsKeys.XPubKey))
                , Times.Once
            );
            mockSettings.Verify(s => s.GetValueAsync<bool>(It.Is<string>(i => i == SettingsKeys.UseSegwit))
                , Times.Once
            );
        }

        [Fact]
        public async Task SaveSettingsAsyncCallsSettingsProviderAsync()
        {
            var vm = Get(out var mockSettings);

            mockSettings.Setup(
                s => s.SetSecureValueAsync(It.Is<string>(i => i == SettingsKeys.XPubKey), It.IsAny<string>())
            ).Returns(Task.CompletedTask);
            mockSettings.Setup(s =>
                    s.SetValueAsync(It.Is<string>(i => i == SettingsKeys.UseSegwit), It.IsAny<bool>()))
                .Returns(Task.CompletedTask);
            mockSettings.Setup(
                s => s.SetValueAsync(It.Is<string>(i => i == SettingsKeys.LastId), It.IsAny<long>())
            ).Returns(Task.CompletedTask);

            await vm.SaveSettingsAsync();

            mockSettings.Verify(
                s => s.SetSecureValueAsync(It.Is<string>(i => i == SettingsKeys.XPubKey), It.IsAny<string>())
                , Times.Once
            );

            mockSettings.Verify(
                s => s.SetValueAsync(It.Is<string>(i => i == SettingsKeys.LastId), It.IsAny<long>())
                , Times.Once
            );
            mockSettings.Verify(s => s.SetValueAsync(It.Is<string>(i => i == SettingsKeys.UseSegwit), It.IsAny<bool>())
                , Times.Once
            );
        }
    }
}
