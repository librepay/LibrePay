using System;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.ViewModels.Base;
using Xamarin.Forms;

namespace BitcoinPOS_App.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private readonly ISettingsProvider _settingsProvider;

        public bool IsLoaded { get; set; }

        private string _extendedPublicKey;

        private string _settingUseSegwit;

        public string ExtendedPublicKey
        {
            get => _extendedPublicKey;
            set => SetProperty(ref _extendedPublicKey, value);
        }

        public string SettingUseSegwit {
            get => _settingUseSegwit;
            set => SetProperty(ref _settingUseSegwit, value);
        }


        public SettingsPageViewModel(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task LoadSettingsAsync()
        {
            // tries to fetch the extended public key
            // then if it work changes the IsLoaded prop as true
            // if it doesn't work shows a message that get users attention
            await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsXPubKey)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        MessagingCenter.Send<SettingsPageViewModel, Exception>(
                            this
                            , MessengerKeys.SettingsFailedLoadSettings
                            , t.Exception
                        );

                        return Task.CompletedTask;
                    }

                    IsLoaded = true;
                    ExtendedPublicKey = t.Result;

                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);

            // same as above, for fetching if we will use Segregated Witness addresses
            await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsUseSegwit)
                .ContinueWith(t => {
                    if (t.IsCanceled || t.IsFaulted) {
                        MessagingCenter.Send<SettingsPageViewModel, Exception>(
                            this
                            , MessengerKeys.SettingsFailedLoadSettings
                            , t.Exception
                        );

                        return Task.CompletedTask;
                    }

                    IsLoaded = true;
                    SettingUseSegwit = t.Result;

                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);
        }

        public async Task SaveSettingsAsync()
        {
            // Saves the current extended public key
            await _settingsProvider.SetSecureValueAsync(Constants.SettingsXPubKey, ExtendedPublicKey)
                .ConfigureAwait(false);

            // Saves if we will use Segwit adresses or not
            await _settingsProvider.SetSecureValueAsync(Constants.SettingsUseSegwit, SettingUseSegwit)
                .ConfigureAwait(false);

            // Saves the last sequential ID (path) used.
            await _settingsProvider.SetValueAsync(Constants.LastId, 0L)
                .ConfigureAwait(false);
        }
    }
}
