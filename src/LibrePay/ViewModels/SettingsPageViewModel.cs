using System;
using System.Globalization;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.ViewModels.Base;
using Xamarin.Forms;

namespace LibrePay.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly CultureInfo _cultureInfo;

        public bool IsLoaded { get; set; }

        private string _extendedPublicKey;

        private bool _useSegwit;

        public string ExtendedPublicKey
        {
            get => _extendedPublicKey;
            set => SetProperty(ref _extendedPublicKey, value);
        }

        public bool UseSegwit
        {
            get => _useSegwit;
            set => SetProperty(ref _useSegwit, value);
        }


        public string CurrentCultureView
            => $"{_cultureInfo.IetfLanguageTag} - {new RegionInfo(_cultureInfo.LCID).ISOCurrencySymbol}";

        public SettingsPageViewModel(
            ISettingsProvider settingsProvider
            , CultureInfo cultureInfo
        )
        {
            _settingsProvider = settingsProvider;
            _cultureInfo = cultureInfo;
        }

        //TODO: This needs rework
        public async Task LoadSettingsAsync()
        {
            // tries to fetch the extended public key
            // then if it work changes the IsLoaded prop as true
            // if it doesn't work shows a message that get users attention
            await _settingsProvider.GetSecureValueAsync<string>(SettingsKeys.XPubKey)
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
            await _settingsProvider.GetValueAsync<bool>(SettingsKeys.UseSegwit)
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
                    UseSegwit = t.Result;

                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);
        }

        public async Task SaveSettingsAsync()
        {
            // Saves the current extended public key
            await _settingsProvider.SetSecureValueAsync(SettingsKeys.XPubKey, ExtendedPublicKey)
                .ConfigureAwait(false);

            // Saves if we will use Segwit addresses or not
            await _settingsProvider.SetValueAsync(SettingsKeys.UseSegwit, UseSegwit)
                .ConfigureAwait(false);

            // Saves the last sequential ID (path) used.
            await _settingsProvider.SetValueAsync(SettingsKeys.LastId, 0L)
                .ConfigureAwait(false);
        }
    }
}
