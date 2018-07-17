using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;

namespace BitcoinPOS_App.Views
{
    public partial class MainPage : ContentPage
    {
        private MainPageViewModel _viewModel;
        private readonly IMessageDisplayer _msgDisplayer;
        private readonly ISettingsProvider _settingsProvider;

        public Command CheckPrivateKey { get; set; }

        public MainPage()
        {
            InitializeComponent();

            ResetViewModel();
            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();
            _settingsProvider = DependencyService.Get<ISettingsProvider>();

            CheckPrivateKey = new Command(async () =>
            {
                Debug.WriteLine("Checkando se tem private key configurada...");
                var privateKey = await _settingsProvider.GetSecureValueAsync<string>(Constants.SettingPrivateKey);

                if (string.IsNullOrWhiteSpace(privateKey))
                {
                    Debug.WriteLine("Pedindo para o usuário configurar a private key");

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var alertResult = await DisplayAlert("Configurações",
                            "Para usar o aplicativo é necessário configurar uma chave privada para poder gerar endereços de pagamento.",
                            "Ok", "Cancelar");

                        if (alertResult)
                            await Navigation.PushAsync(new SettingsPage());
                    });
                }

                privateKey = null;
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Task.Delay(TimeSpan.FromSeconds(1))
                .ContinueWith(_ => CheckPrivateKey.Execute(null));
        }

        private void ResetViewModel()
        {
            BindingContext = _viewModel = new MainPageViewModel();
        }

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void Number_Clicked(object sender, EventArgs e)
        {
            var bt = sender as Button;

            if (bt == null)
                return;

            Debug.WriteLine($"Botão apertado: {bt.AutomationId}");

            if (bt.AutomationId == "virgula" && _viewModel.TransactionValueStr.Contains(","))
                return;

            _viewModel.TransactionValueStr = _viewModel.TransactionValueStr + bt.Text;

            // verifies if there's more than 3 digits after the decimal separator
            // and if there's any shows a toast
            var values = _viewModel.TransactionValueStr.Split(',');
            if (values.Length > 1 && values[1].Length == 3)
            {
                Debug.WriteLine("Mostrou msg de valor arredondado");
                await _msgDisplayer.ShowMessageAsync("O valor será arredondado!");
            }

            Debug.WriteLine($"Novo valor: {_viewModel.TransactionValueStr}");
        }

        private void Pay_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine($"Pagar pressionado: {_viewModel.TransactionValue}");

            if (_viewModel.TransactionValue == 0)
                return;

            Debug.WriteLine("Seguindo com o pagamento");
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Limpar pressionado");
            ResetViewModel();
        }
    }
}