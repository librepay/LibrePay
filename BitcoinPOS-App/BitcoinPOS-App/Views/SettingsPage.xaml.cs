using System;
using System.Diagnostics;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage
    {
        private readonly SettingsViewModel _viewModel;
        private readonly IMessageDisplayer _messageDisplayer;

        public SettingsPage(SettingsViewModel viewModel, IMessageDisplayer messageDisplayer)
        {
            InitializeComponent();

            _messageDisplayer = messageDisplayer;

            BindingContext = _viewModel = viewModel;

            MessagingCenter.Subscribe<SettingsViewModel, Exception>(_viewModel
                , MessengerKeys.SettingsFailedLoadSettings
                , (_, ex) =>
                {
                    Debug.WriteLine($"Erro ao buscar xpub: {Environment.NewLine}{ex}");

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        // needs to be at root to show alerts
                        await Navigation.PopToRootAsync();
                        await DisplayAlert("Erro", "Erro ao carregar configuração", "Cancelar");
                    });
                }
            );
        }

        protected override void OnAppearing()
        {
            //HACK: Editor doesn't allow selection (https://forums.xamarin.com/discussion/100000/editor-with-textproperty-does-not-support-text-selection)
            EdtXPub.IsEnabled = true;

            base.OnAppearing();

            _viewModel.LoadSettingsAsync()
                .ConfigureAwait(false);
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            await _viewModel.SaveSettingsAsync();

            // go back to the main page
            await Navigation.PopAsync();
            await _messageDisplayer.ShowMessageAsync("Configurações salvas!");
        }
    }
}