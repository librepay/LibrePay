using System;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BitcoinPOS_App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PaymentFinalizationPage
    {
        private readonly PaymentFinalizationViewModel _viewModel;
        private readonly IMessageDisplayer _msgDisplayer;

        public PaymentFinalizationPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new PaymentFinalizationViewModel();

            _msgDisplayer = DependencyService.Get<IMessageDisplayer>();
        }

        public PaymentFinalizationPage(PaymentFinalizationViewModel viewModel) : this()
        {
            BindingContext = _viewModel = viewModel;
        }

        protected override bool OnBackButtonPressed()
        {
            // while the payment isn't finished
            // don't let the user go back
            if (_viewModel.Payment.Done)
            {
                return base.OnBackButtonPressed();
            }

            _msgDisplayer.ShowMessageAsync("Pagamento ainda não confirmado...");
            return true;
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void Ok_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}