using System;
using BitcoinPOS_App.Views;
using NBitcoin;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace BitcoinPOS_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Adds entropy to the random utility used by NBitcoin
            RandomUtils.AddEntropy($"bitcoin-pos-app-{DateTime.Now.Ticks}-{Device.RuntimePlatform}");

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}