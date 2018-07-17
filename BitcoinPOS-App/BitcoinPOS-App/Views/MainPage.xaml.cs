using System;
using System.Diagnostics;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;

namespace BitcoinPOS_App.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private readonly IMessageDisplayer _msgDisplayer;

        public MainPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new MainPageViewModel();
            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();
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
                await _msgDisplayer.ShowMessageAsync("O valor será arredondado!");
            }

            Debug.WriteLine($"Novo valor: {_viewModel.TransactionValueStr}");
        }

        private void Pay_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine($"Pagar: {_viewModel.TransactionValue}");
        }
    }
}