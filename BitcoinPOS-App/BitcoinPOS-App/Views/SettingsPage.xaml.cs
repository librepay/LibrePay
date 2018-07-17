using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private SettingsViewModel _viewModel;

        public Command LoadSettingsCommand { get; }

        private readonly ISettingsProvider _settingsProvider;
        private readonly IMessageDisplayer _msgDisplayer;

        public SettingsPage()
        {
            InitializeComponent();

            LoadSettingsCommand = new Command(LoadSettingsCommandAction);

            _settingsProvider = DependencyService.Get<ISettingsProvider>();
            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();
            BindingContext = _viewModel = new SettingsViewModel();

            LoadSettingsCommand.Execute(null);
        }

        private void LoadSettingsCommandAction()
        {
            // tries to fetch the private key
            // then if it work changes the IsLoaded prop as true
            // if it doesn't work shows a message that get users attention
            _settingsProvider.GetSecureValueAsync<string>(Constants.SettingPrivateKey)
                .ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        _viewModel.IsLoaded = true;
                        _viewModel.PrivateKey = t.Result;
                        return Task.CompletedTask;
                    }

                    Debug.WriteLine(string.Format("Erro ao buscar private key{0}{1}", Environment.NewLine, t.Exception));

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        // needs to be at root to show alerts
                        await Navigation.PopToRootAsync();
                        await DisplayAlert("Erro", "Erro ao carregar configuração", "Cancelar");
                    });
                    return Task.CompletedTask;
                });
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            // saves the current private key
            await _settingsProvider.SetSecureValueAsync(Constants.SettingPrivateKey, _viewModel.PrivateKey);

            // go back to the main page
            await Navigation.PopAsync();
            await _msgDisplayer.ShowMessageAsync("Configurações salvas!");
        }
    }
}