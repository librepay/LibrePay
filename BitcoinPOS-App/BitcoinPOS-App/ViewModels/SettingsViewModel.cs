namespace BitcoinPOS_App.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public bool IsLoaded { get; set; }

        private string privateKey;
        public string PrivateKey
        {
            get { return privateKey; }
            set { SetProperty(ref privateKey, value); }
        }
    }
}
