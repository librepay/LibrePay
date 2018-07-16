using System;
using Xamarin.Forms;
using System.Diagnostics;
using BitcoinPOS_App.ViewModels;

namespace BitcoinPOS_App.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new MainPageViewModel();
        }

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private void Number_Clicked(object sender, EventArgs e)
        {
            var bt = sender as Button;

            if (bt == null)
                return;

            Debug.WriteLine($"Botão apertado: {bt.AutomationId}");

            if (short.TryParse(bt.AutomationId, out var num))
            {
                _viewModel.ResultText += num;
            }
            else
            {
                Debug.WriteLine($"Erro ao converter {nameof(bt)}.{nameof(bt.AutomationId)}");
            }
        }
    }
}