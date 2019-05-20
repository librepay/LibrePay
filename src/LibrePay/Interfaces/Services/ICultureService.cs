using System.Globalization;
using System.Threading.Tasks;

namespace LibrePay.Interfaces.Services
{
    public interface ICultureService
    {
        CultureInfo CurrentCultureInfo { get; }

        Task ResetCultureInfoAsync(CultureInfo cultureInfo);

        CultureInfo[] GetAllCultures();
    }
}
