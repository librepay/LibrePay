using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.ViewModels;
using System;
using System.Threading.Tasks;
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

            LoadSettingsCommand = new Command(() =>
            {
                _settingsProvider.GetSecureValueAsync<string>(Constants.Setting_PrivateKey)
                    .ContinueWith(_ => _viewModel.IsLoaded = true, TaskContinuationOptions.OnlyOnRanToCompletion)
                    .ContinueWith(_ => DisplayAlert("Erro", "Erro ao carregar configuração", "Cancelar"), TaskContinuationOptions.NotOnRanToCompletion);
            });

            _settingsProvider = DependencyService.Get<ISettingsProvider>();
            //_msgDisplayer = DependencyService.Get<IMessageDisplayer>(); ;
            BindingContext = _viewModel = new SettingsViewModel();

            LoadSettingsCommand.Execute(null);
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            await _settingsProvider.SetSecureValueAsync(Constants.Setting_PrivateKey, _viewModel.PrivateKey);
            await Navigation.PopAsync();
        }
    }
}