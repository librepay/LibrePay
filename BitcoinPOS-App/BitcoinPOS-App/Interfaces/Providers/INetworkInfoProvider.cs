using System;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.Interfaces.Providers
{
    public interface INetworkInfoProvider
    {
        BackgroundJob WaitAddressReceiveAnyTransactionAsync(string address, Action onReceiveAnyTx);
    }
}