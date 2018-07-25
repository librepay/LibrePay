using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Devices;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.ViewModels;
using NBitcoin;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage
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

#if DEBUG
            var button = new Button
            {
                Text = "Criar xpub aleatório"
            };
            button.Clicked += (sender, args) =>
            {
                // create a mnemonic to easy the development process
                // as almost every wallet accepts it
                var mnemonic = new Mnemonic(Wordlist.English);
                var extKey = mnemonic.DeriveExtKey();
                var xpubKey = extKey.Neuter();

                Debug.WriteLine($"Debug mnemonic: {mnemonic}");
                Debug.WriteLine($"Debug xpriv: {extKey.ToString(Network.TestNet)}");
                Debug.WriteLine($"Debug xpub: {xpubKey.ToString(Network.TestNet)}");

                _settingsProvider.SetSecureValueAsync(Constants.SettingsXPubKey, xpubKey.ToString(Network.TestNet));

                // reload settings
                LoadSettingsCommand.Execute(null);
            };
            SettingsStack.Children.Add(button);
#endif
        }

        protected override void OnAppearing()
        {
            //HACK: Editor doesn't allow selection (https://forums.xamarin.com/discussion/100000/editor-with-textproperty-does-not-support-text-selection)
            edtXPub.IsEnabled = true;

            base.OnAppearing();
        }

        private void LoadSettingsCommandAction()
        {
            // tries to fetch the extended public key
            // then if it work changes the IsLoaded prop as true
            // if it doesn't work shows a message that get users attention
            _settingsProvider.GetSecureValueAsync<string>(Constants.SettingsXPubKey)
                .ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        _viewModel.IsLoaded = true;
                        _viewModel.ExtendedPublicKey = t.Result;
                        return Task.CompletedTask;
                    }

                    Debug.WriteLine($"Erro ao buscar xpub: {Environment.NewLine}{t.Exception}");

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
            // saves the current extended public key
            await _settingsProvider.SetSecureValueAsync(Constants.SettingsXPubKey, _viewModel.ExtendedPublicKey);
            await _settingsProvider.SetValueAsync(Constants.LastId, 0L);

            // go back to the main page
            await Navigation.PopAsync();
            await _msgDisplayer.ShowMessageAsync("Configurações salvas!");
        }
    }
}