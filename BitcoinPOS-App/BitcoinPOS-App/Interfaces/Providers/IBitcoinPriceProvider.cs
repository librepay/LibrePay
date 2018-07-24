using System.Threading.Tasks;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.Interfaces.Providers
{
    public interface IBitcoinPriceProvider
    {
        Task<ExchangeRate> GetLocalBitcoinPrice();
    }
}