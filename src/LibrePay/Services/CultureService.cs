using System.Globalization;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.Interfaces.Services;
using Xamarin.Forms;

namespace LibrePay.Services
{
    public class CultureService : ICultureService
    {
        private readonly ISettingsProvider _settingsProvider;
        public CultureInfo CurrentCultureInfo { get; }

        public CultureService(
            CultureInfo cultureInfo
            , ISettingsProvider settingsProvider
        )
        {
            _settingsProvider = settingsProvider;
            CurrentCultureInfo = cultureInfo;
        }

        public async Task ResetCultureInfoAsync(CultureInfo cultureInfo)
        {
            await _settingsProvider.SetValueAsync(SettingsKeys.CultureTag, cultureInfo.IetfLanguageTag)
                .ConfigureAwait(false);

            // resets application
            var app = GetApp();
            app.Init();
        }

        public CultureInfo[] GetAllCultures()
            => CultureInfo.GetCultures(CultureTypes.AllCultures);

        public virtual App GetApp()
            => (App) Application.Current;
    }
}
