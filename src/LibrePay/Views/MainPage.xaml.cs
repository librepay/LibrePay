using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LibrePay.Interfaces.Devices;
using LibrePay.Interfaces.Services.Navigation;
using LibrePay.Models;
using LibrePay.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LibrePay.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly INavigationService _navigationService;
        private readonly IMessageDisplayer _msgDisplayer;

        public MainPage(
            MainPageViewModel viewModel
            , INavigationService navigationService
            , IMessageDisplayer msgDisplayer
        )
        {
            InitializeComponent();

            BindingContext = _viewModel = viewModel;

            _navigationService = navigationService;
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
            await _navigationService.NavigateToAsync<SettingsPageViewModel>();
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
                await _navigationService.NavigateToModalAsync<PaymentFinalizationPageViewModel>(payment);
            }
        }

        private void Clean_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Limpar pressionado", "UI");
            _viewModel.BackspacePress();
        }

        private void Clean_OnLongPressed(object sender, EventArgs e)
        {
            Debug.WriteLine("Limpar segurado", "UI");
            _viewModel.ResetPinpad();
        }

        private static bool _amountSwitchAnimationLock;

        private async void AmountSwitchTapped(object sender, EventArgs e)
        {
            if (_amountSwitchAnimationLock)
                return;

            _amountSwitchAnimationLock = true;

            var upOrDown = _viewModel.SwitchEnteringSymbol();

            await Task.Yield();

            var grid = (Grid)sender;
            var labelPrevious = (Label)grid.Children.Last();
            var labelNext = (Label)grid.Children.First();

            const string switchValueEntryAnimationName = "switchValueEntry";
            this.AbortAnimation(switchValueEntryAnimationName);

            const int translateYCoefficient = 30;
            int translateYStart = upOrDown ? -translateYCoefficient : -translateYCoefficient / 4,
                translateYEnd = upOrDown ? -translateYCoefficient / 4 : -translateYCoefficient;

            var defaultEasing = Easing.SpringOut;
            var fullAnimation = new Animation()
                    // font size
                    .WithConcurrent(new Animation(
                        d => labelNext.FontSize = d
                        , labelNext.FontSize
                        , labelPrevious.FontSize
                        , defaultEasing
                    ))
                    .WithConcurrent(new Animation(
                        d => labelPrevious.FontSize = d
                        , labelPrevious.FontSize
                        , labelNext.FontSize
                        , defaultEasing
                    ))

                    // translate y
                    .WithConcurrent(new Animation(
                        d => labelNext.TranslationY = d
                        , translateYStart
                        , upOrDown ? translateYEnd + translateYCoefficient / 4 : translateYEnd
                        , upOrDown ? Easing.BounceOut : Easing.CubicOut
                    ))
                    .WithConcurrent(new Animation(
                        d => labelPrevious.TranslationY = d
                        , translateYStart
                        , upOrDown ? translateYEnd : translateYEnd + -translateYCoefficient / 4
                        , upOrDown ? Easing.CubicOut : Easing.BounceOut
                    ))

                    // font color
                    .WithConcurrent(
                        labelNext.ColorTo(
                            labelNext.TextColor
                            , labelPrevious.TextColor
                            , c => labelNext.TextColor = c
                            , defaultEasing
                        )
                    )
                    .WithConcurrent(
                        labelPrevious.ColorTo(
                            labelPrevious.TextColor
                            , labelNext.TextColor
                            , c => labelPrevious.TextColor = c
                            , defaultEasing
                        )
                    )
                ;

            await this.RunAnimationAsync(switchValueEntryAnimationName, fullAnimation, length: 500);

            _amountSwitchAnimationLock = false;
        }
    }
}
