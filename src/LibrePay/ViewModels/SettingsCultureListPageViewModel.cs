using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using LibrePay.Interfaces.Services;
using LibrePay.ViewModels.Base;
using LibrePay.Wrappers;

namespace LibrePay.ViewModels
{
    public class SettingsCultureListPageViewModel : BaseViewModel
    {
        private readonly ICultureService _cultureService;
        private readonly CultureInfo[] _allCultures;

        private ObservableCollection<CultureInfoWrapper> _culturesAndCurrencies;
        private string _filter;

        public string Filter
        {
            get => _filter;
            set => SetProperty(ref _filter, value);
        }

        public ObservableCollection<CultureInfoWrapper> CulturesAndCurrencies
        {
            get => _culturesAndCurrencies;
            set => SetProperty(ref _culturesAndCurrencies, value);
        }

        public SettingsCultureListPageViewModel(
            ICultureService cultureService
        )
        {
            _cultureService = cultureService;

            _allCultures = _cultureService.GetAllCultures()
                .Where(c => !c.IsNeutralCulture)
                .ToArray();

            FilterCulturesList();

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Filter))
            {
                FilterCulturesList();
            }
        }

        private void FilterCulturesList()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                CulturesAndCurrencies = new ObservableCollection<CultureInfoWrapper>(
                    _allCultures
                        .OrderBy(c => c.NativeName)
                        .Select(c => new CultureInfoWrapper(c))
                );
            }
            else
            {
                CulturesAndCurrencies = new ObservableCollection<CultureInfoWrapper>(
                    _allCultures
                        .Where(c => c.NativeName.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        .OrderBy(c => c.NativeName)
                        .Select(c => new CultureInfoWrapper(c))
                );
            }
        }

        public void SetSelectedCultureInfo(CultureInfo cultureInfo)
        {
            RunOnMainThread(async () => await _cultureService.ResetCultureInfoAsync(cultureInfo));
        }
    }
}
