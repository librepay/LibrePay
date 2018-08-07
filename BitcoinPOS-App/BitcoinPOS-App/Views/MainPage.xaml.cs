using System;
using System.Diagnostics;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly IMessageDisplayer _msgDisplayer;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;

            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();

            MessagingCenter.Subscribe<MainPageViewModel, bool>(
                _viewModel
                , MessengerKeys.MainCheckXPubExistence
                , (_, xpubExists) =>
                {
                    if (xpubExists)
                        return;

                    Debug.WriteLine("Pedindo para o usuário configurar a xpub key", "UI");

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var alertResult = await DisplayAlert(
                            "Configurações"
                            , "Para usar o aplicativo é necessário configurar uma Extended Public Key" +
                              " para poder gerar endereços de pagamento."
                            , "Ok"
                            , "Cancelar"
                        );

                        if (alertResult)
                            await Navigation.PushAsync(new SettingsPage());
                    });
                }
            );
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.InitializeAsync()
                .ConfigureAwait(false);
        }

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void Receive_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine($"Pagar pressionado: {_viewModel.TransactionValue}", "UI");

            PaymentFinalizationViewModel paymentViewModel = null;
            try
            {
                paymentViewModel = await _viewModel.PrepareToReceivePayment();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await _msgDisplayer.ShowMessageAsync("Aconteceu um erro.");
            }

            if (paymentViewModel != null)
            {
                await Navigation.PushModalAsync(
                    new PaymentFinalizationPage(paymentViewModel)
                );
            }
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Limpar pressionado", "UI");
            _viewModel.ResetPinpad();
        }
    }
}