using LibrePay.Interfaces.Services.Navigation;
using LibrePay.ViewModels;
using LibrePay.Wrappers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LibrePay.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsCultureListPage
    {
        private readonly INavigationService _navigationService;
        private readonly SettingsCultureListPageViewModel _viewModel;

        public SettingsCultureListPage(
            SettingsCultureListPageViewModel viewModel
            , INavigationService navigationService
        )
        {
            _navigationService = navigationService;
            BindingContext = _viewModel = viewModel;

            InitializeComponent();
        }

        private async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var cultureInfo = ((CultureInfoWrapper) e.Item).CultureInfo;

            var result = await DisplayAlert(
                "Culture selected"
                , $"You've selected: \"{cultureInfo.IetfLanguageTag} - {cultureInfo.NativeName}\""
                , "OK"
                , "Cancel"
            );

            if (result)
            {
                _viewModel.SetSelectedCultureInfo(cultureInfo);
                await _navigationService.PopModalStackAsync();
            }

            //Deselect Item
            ((ListView) sender).SelectedItem = null;
        }
    }
}
