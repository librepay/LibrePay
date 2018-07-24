using System;
using System.Collections.Generic;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.Interfaces.Providers
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
        BackgroundJob WaitCompletePayment(Payment payment, Action<decimal> onComplete);
    }
}