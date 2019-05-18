using System.Threading.Tasks;
using LibrePay.Models;

namespace LibrePay.Interfaces.Providers
{
    public interface IBitcoinPriceProvider
    {
        Task<ExchangeRate> GetLocalBitcoinPrice();
    }
}