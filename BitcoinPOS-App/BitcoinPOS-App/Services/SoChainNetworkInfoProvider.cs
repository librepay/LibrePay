using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using BitcoinPOS_App.Interfaces;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.Services;
using NBitcoin;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(SoChainNetworkInfoProvider))]

namespace BitcoinPOS_App.Services
{
    /// <summary>
    /// Uses SoChain's API (https://chain.so/api) to get network information
    /// </summary>
    public class SoChainNetworkInfoProvider : INetworkInfoProvider
    {
        private static readonly HttpClient HttpClient;
        private static readonly string SoChainNetwork;

        static SoChainNetworkInfoProvider()
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://chain.so/api/v2"),
                DefaultRequestHeaders =
                {
                    Accept = {new MediaTypeWithQualityHeaderValue("application/json")}
                },
                Timeout = TimeSpan.FromSeconds(15)
            };

            if (Constants.NetworkInUse == Network.Main)
            {
                SoChainNetwork = "BTC";
            }
            else if (Constants.NetworkInUse == Network.TestNet)
            {
                SoChainNetwork = "BTCTEST";
            }
            else
            {
                SoChainNetwork = null;
                Debug.WriteLine("ERRO: Network desconhecida!!!");
            }
        }

        public BackgroundJob WaitAddressReceiveAnyTransactionAsync(string address, Action onReceiveAnyTx)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (onReceiveAnyTx == null) throw new ArgumentNullException(nameof(onReceiveAnyTx));

            var thread = new Thread(async () =>
            {
                while (true)
                {
                    var response = await HttpClient.GetAsync($"/api/v2/get_tx_received/{SoChainNetwork}/{address}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jobj = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var txs = jobj["data"]["txs"];

                        if (txs is JArray txArr && txArr.Count > 0)
                        {
                            try
                            {
                                onReceiveAnyTx();
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(
                                    $"ERRO: Falha no callback ({nameof(WaitAddressReceiveAnyTransactionAsync)}):" + e
                                );
                            }

                            break;
                        }
                    }
#if DEBUG
                    else
                    {
                        Debug.WriteLine(
                            $"ERRO: Falha ao buscar dados da API do SoChain.{Environment.NewLine}" +
                            await response.Content.ReadAsStringAsync()
                        );
                    }
#endif
                    // wait for the next tick
                    Thread.Sleep(5000);
                }
            })
            {
                Name = "so-chain-addr-check",
                IsBackground = true
            };

            thread.Start();

            return new BackgroundJob(thread);
        }
    }
}