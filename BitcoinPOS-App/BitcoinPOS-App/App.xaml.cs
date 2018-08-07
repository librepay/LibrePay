using System;
using System.Diagnostics;
using BitcoinPOS_App.ViewModels;
using BitcoinPOS_App.Views;
using NBitcoin;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace BitcoinPOS_App
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            // Adds entropy to the random utility used by NBitcoin
            RandomUtils.AddEntropy($"bitcoin-pos-app-{DateTime.Now.Ticks}-{Device.RuntimePlatform}");

            MainPage = new NavigationPage(new MainPage(new MainPageViewModel()));
        }

        protected override void OnStart()
        {
            Debug.WriteLine("Iniciando...", "APP");
        }

        protected override void OnSleep()
        {
            Debug.WriteLine("Dormindo...", "APP");
        }

        protected override void OnResume()
        {
            Debug.WriteLine("Resumindo...", "APP");
        }
    }
}