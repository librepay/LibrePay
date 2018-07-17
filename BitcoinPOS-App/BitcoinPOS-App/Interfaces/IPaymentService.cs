using System.Threading.Tasks;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.Interfaces
{
    public interface IPaymentService
    {
        Task Pay(Payment payment);
    }
}