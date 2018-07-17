namespace BitcoinPOS_App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public bool IsLoaded { get; set; }

        private string _extendedPublicKey;
        public string ExtendedPublicKey
        {
            get => _extendedPublicKey;
            set => SetProperty(ref _extendedPublicKey, value);
        }
    }
}
