using System;
using System.Diagnostics;
using LibrePay.Interfaces.Devices;
using LibrePay.Interfaces.Services.Navigation;
using LibrePay.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LibrePay.Views {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage {
        private readonly SettingsPageViewModel _viewModel;
        private readonly INavigationService _navigationService;
        private readonly IMessageDisplayer _messageDisplayer;

        public SettingsPage(
            SettingsPageViewModel viewModel
            , INavigationService navigationService
            , IMessageDisplayer messageDisplayer
        ) {
            InitializeComponent();

            _navigationService = navigationService;
            _messageDisplayer = messageDisplayer;

            BindingContext = _viewModel = viewModel;

            MessagingCenter.Subscribe<SettingsPageViewModel, Exception>(_viewModel
                , MessengerKeys.SettingsFailedLoadSettings
                , (_, ex) => {
                    Debug.WriteLine($"Erro ao buscar xpub: {Environment.NewLine}{ex}");

                    Device.BeginInvokeOnMainThread(async () => {
                        // needs to be at root to show alerts
                        await _navigationService.ClearStack();
                        await DisplayAlert("Erro", "Erro ao carregar configuração", "Cancelar");
                    });
                }
            );
        }

        protected override void OnAppearing() {
            //HACK: Editor doesn't allow selection (https://forums.xamarin.com/discussion/100000/editor-with-textproperty-does-not-support-text-selection)
            EdtXPub.IsEnabled = true;
            EdtSegWit.IsEnabled = true;

            base.OnAppearing();

            _viewModel.LoadSettingsAsync().ConfigureAwait(false);
        }

        private async void Save_Clicked(object sender, EventArgs e) {
            await _viewModel.SaveSettingsAsync();

            // go back in the navigation stack
            await _navigationService.PopStackAsync();
            await _messageDisplayer.ShowMessageAsync("Configurações salvas!");
        }
    }
}
