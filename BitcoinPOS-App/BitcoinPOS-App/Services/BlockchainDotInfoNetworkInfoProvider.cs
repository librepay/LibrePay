using System;
using System.Diagnostics;
using System.Threading;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.Models;
using WebSocketSharp;

namespace BitcoinPOS_App.Services
{
    /// <summary>
    /// Not in use because there isn't a way to use in Testnet3
    /// </summary>
    public class BlockchainDotInfoNetworkInfoProvider : INetworkInfoProvider
    {
        public BackgroundJob WaitAddressReceiveAnyTransactionAsync(string address, Action onReceiveAnyTx)
        {
            //TODO: Fix
            using (var wsClient = new WebSocket("wss://ws.blockchain.info/inv"))
            {
                try
                {
                    var finished = false;
                    wsClient.OnOpen += (_, e) =>
                    {
                        Debug.WriteLine("[WS] Iniciou conexão.");

                        var json = @"{""op"":""addr_sub"",""addr"":\""" + address + "\"}";
                        wsClient.Send(json);
                    };
                    wsClient.OnClose += (_, e) => Debug.WriteLine($"[WS] Finalizou conexão. {e.Reason}");
                    wsClient.OnError += (_, e) => Debug.WriteLine($"[WS] Erro: {e.Exception}");
                    wsClient.OnMessage += (_, e) =>
                    {
                        Debug.WriteLine($"[WS] Nova mensagem: {e.Data}");
                        finished = true;
                    };
                    wsClient.ConnectAsync();

                    while (!finished)
                    {
                        Thread.Sleep(500);
                    }
                }
                finally
                {
                    if (wsClient.IsAlive)
                    {
                        wsClient.CloseAsync(CloseStatusCode.Normal);
                    }
                }
            }

            return null;
        }
    }
}