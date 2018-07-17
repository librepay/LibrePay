using System.Threading.Tasks;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>
        /// Generate a address for a payment
        /// </summary>
        /// <param name="payment">Payment to have the newly created address</param>
        /// <returns><paramref name="payment"/></returns>
        /// <exception cref="System.ArgumentNullException">when <paramref name="payment"/> is null</exception>
        Task<Payment> GeneratePaymentAddressAsync(Payment payment);
    }
}