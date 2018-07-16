namespace BitcoinPOS_App.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private string resultText = string.Empty;

        public string ResultText
        {
            get => resultText;
            set { SetProperty(ref resultText, value); }
        }
    }
}
