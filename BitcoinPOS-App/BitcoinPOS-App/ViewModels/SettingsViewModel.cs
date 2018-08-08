using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.ViewModels.Base;
using Xamarin.Forms;

namespace BitcoinPOS_App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsProvider _settingsProvider;

        public bool IsLoaded { get; set; }

        private string _extendedPublicKey;

        public string ExtendedPublicKey
        {
            get => _extendedPublicKey;
            set => SetProperty(ref _extendedPublicKey, value);
        }

        public SettingsViewModel(ISettingsProvider settingsProvider)
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
                    if (!t.IsFaulted)
                    {
                        IsLoaded = true;
                        ExtendedPublicKey = t.Result;

                        return Task.CompletedTask;
                    }

                    MessagingCenter.Send(this, MessengerKeys.SettingsFailedLoadSettings, t.Exception);

                    return Task.CompletedTask;
                })
                .ConfigureAwait(false);
        }

        public async Task SaveSettingsAsync()
        {
            // saves the current extended public key
            await _settingsProvider.SetSecureValueAsync(Constants.SettingsXPubKey, ExtendedPublicKey)
                .ConfigureAwait(false);
            await _settingsProvider.SetValueAsync(Constants.LastId, 0L)
                .ConfigureAwait(false);
        }
    }
}