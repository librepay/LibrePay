using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.Models;
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

        public MainPage(MainPageViewModel viewModel, IMessageDisplayer msgDisplayer)
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;

            _msgDisplayer = msgDisplayer;

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
                            await OpenSettingsPageAsync();
                    });
                }
            );
        }

        private async Task OpenSettingsPageAsync()
        {
            await Navigation.PushAsync(App.Container.Resolve<SettingsPage>());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.InitializeAsync()
                .ConfigureAwait(false);
        }

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await OpenSettingsPageAsync();
        }

        private async void Receive_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine($"Pagar pressionado: {_viewModel.TransactionValue}", "UI");

            Payment payment = null;
            try
            {
                payment = await _viewModel.GenerateNewPayment();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await _msgDisplayer.ShowMessageAsync("Aconteceu um erro.");
            }

            if (payment != null)
            {
                using (var scope = App.Container.BeginLifetimeScope())
                {
                    var vm = scope.Resolve<PaymentFinalizationViewModel>();
                    vm.Payment = payment;

                    await Navigation.PushModalAsync(
                        scope.Resolve<PaymentFinalizationPage>(
                            new TypedParameter(typeof(PaymentFinalizationViewModel), vm)
                        )
                    );
                }
            }
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Limpar pressionado", "UI");
            _viewModel.ResetPinpad();
        }
    }
}