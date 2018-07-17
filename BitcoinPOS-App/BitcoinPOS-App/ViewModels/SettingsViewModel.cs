namespace BitcoinPOS_App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public bool IsLoaded { get; set; }

        private string _privateKey;
        public string PrivateKey
        {
            get => _privateKey;
            set => SetProperty(ref _privateKey, value);
        }
    }
}
