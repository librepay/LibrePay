using System;
using BitcoinPOS_App.Models;

namespace BitcoinPOS_App.Interfaces
{
    public interface INetworkInfoProvider
    {
        BackgroundJob WaitAddressReceiveAnyTransactionAsync(string address, Action onReceiveAnyTx);
    }
}