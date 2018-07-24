using System.Threading.Tasks;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.ViewModels;

namespace BitcoinPOS_App.Interfaces.Services
{
    public interface IPaymentService
    {
        /// <summary>
        /// Generates a address for a payment
        /// </summary>
        /// <param name="payment">Payment to have the newly created address</param>
        /// <returns><paramref name="payment"/></returns>
        /// <exception cref="System.ArgumentNullException">when <paramref name="payment"/> is null</exception>
        Task<Payment> GeneratePaymentAddressAsync(Payment payment);

        /// <summary>
        /// Generates a new payment and fill all the needed properties
        /// </summary>
        /// <exception cref="System.ArgumentNullException">when <paramref name="viewModel"/> is null</exception>
        Task<Payment> GenerateNewPayment(MainPageViewModel viewModel);
    }
}