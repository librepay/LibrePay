using System;
using LibrePay.Models;

namespace LibrePay.Interfaces.Providers
{
    public interface INetworkInfoProvider
    {
        /// <summary>
        /// Creates a background job to watch the address until it got fulfilled
        /// </summary>
        /// <param name="payment">The payment to be watched</param>
        /// <param name="onComplete">
        /// The action to be taken when the payment is complete.<para />
        /// The first parameter is the amount sent to this address
        /// </param>
        /// <param name="onReceiveTx">
        /// Notifies when an amount is received.
        /// The first parameter is the total amount.
        /// The second is the tx amount.
        /// </param>
        BackgroundJob WaitCompletePayment(
            Payment payment
            , Action<decimal> onComplete
            , Action<decimal, decimal> onReceiveTx = null
        );
    }
}