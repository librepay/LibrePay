using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.Providers;
using NBitcoin;
using Newtonsoft.Json.Linq;
using Polly;
using Xamarin.Forms;
using Transaction = BitcoinPOS_App.Models.Transaction;

[assembly: Dependency(typeof(SoChainNetworkInfoProvider))]

namespace BitcoinPOS_App.Providers
{
    /// <summary>
    /// Uses SoChain's API (https://chain.so/api) to get network information
    /// </summary>
    public class SoChainNetworkInfoProvider : INetworkInfoProvider
    {
        private static readonly HttpClient HttpClient;
        private static readonly Policy<HttpResponseMessage> SoChainPolicy;

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

            var policyBuilder = Policy.HandleResult<HttpResponseMessage>(h => !h.IsSuccessStatusCode)
                .Or<Exception>();

            SoChainPolicy = Policy.WrapAsync(
                policyBuilder.FallbackAsync(new HttpResponseMessage(HttpStatusCode.BadRequest))
                , policyBuilder.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(1))
                // throttle
                , Policy.HandleResult<HttpResponseMessage>(_ => true)
                    .Or<Exception>()
                    .CircuitBreakerAsync(1, TimeSpan.FromSeconds(5))
                , Policy.BulkheadAsync<HttpResponseMessage>(1)
                , Policy.TimeoutAsync<HttpResponseMessage>(_ => TimeSpan.FromSeconds(5))
            );
        }

        public BackgroundJob WaitCompletePayment(
            Payment payment
            , Action<decimal> onComplete
            , Action<decimal, decimal> onReceiveTx = null
        )
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            if (onComplete == null) throw new ArgumentNullException(nameof(onComplete));

            if (payment.Done)
                return null;

            var valueBtc = payment.ValueBitcoin;
            var knownTransactions = new Transaction[0];

            return NotifyTransactionsOfAAddress(payment.Address, transactions =>
            {
                // order txs to facilitate notification
                var txArr = transactions
                    .OrderBy(t => t.Id)
                    .ToArray();

                var totalValue = txArr.Sum(t => t.Value);

                if (totalValue >= valueBtc)
                {
                    onComplete(totalValue);
                    return true;
                }

                if (onReceiveTx != null && txArr.Length != knownTransactions.Length)
                {
                    // notifies new txs
                    var skp = knownTransactions.Length - 1;
                    if (skp < 0)
                        skp = 0;

                    foreach (var tx in txArr.Skip(skp))
                    {
                        onReceiveTx(totalValue, tx.Value);
                    }
                }

                knownTransactions = knownTransactions.Union(txArr).ToArray();
                return false;
            });
        }

        private BackgroundJob NotifyTransactionsOfAAddress(
            string address
            , Func<IEnumerable<Transaction>, bool> onReceiveAnyTx
        )
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (onReceiveAnyTx == null) throw new ArgumentNullException(nameof(onReceiveAnyTx));

            var thread = new Thread(WatchAddressInSoChainApi)
            {
                Name = $"so-chain-addr-checker: {address}",
                IsBackground = true
            };
            thread.Start(new WatcherInfo
            {
                Address = address,
                Callback = onReceiveAnyTx
            });

            return new BackgroundJob(thread);
        }

        private void WatchAddressInSoChainApi(object @param)
        {
            var info = (WatcherInfo) @param;
            var bitcoinAddress = Network.Parse<BitcoinAddress>(info.Address, null);
            var network = bitcoinAddress.Network == Network.Main
                ? "BTC"
                : "BTCTEST";

            while (true)
            {
                var response = SoChainPolicy.ExecuteAsync(() =>
                {
                    Debug.WriteLine($"[API/SoChain]: Realizando chamada. Address: {info.Address}");
                    return HttpClient.GetAsync($"/api/v2/get_tx_received/{network}/{info.Address}");
                }).Result;

                if (!response.IsSuccessStatusCode)
                    continue;

                var jobj = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var txs = jobj["data"]["txs"];

                if (!(txs is JArray txArr) || txArr.Count <= 0)
                    continue;

                var shouldBreak = false;
                try
                {
                    shouldBreak = info.Callback(GetTransactionsFromJArray(txArr));
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"ERRO: Falha no callback ({nameof(NotifyTransactionsOfAAddress)}):" + e);
                }

                if (shouldBreak)
                    break;
            }
        }

        private IEnumerable<Transaction> GetTransactionsFromJArray(JArray txArr)
        {
            foreach (var tx in txArr)
            {
                yield return new Transaction
                {
                    Confirmations = tx.Value<uint>("confirmations"),
                    Value = tx.Value<decimal>("value"),
                    Id = tx.Value<string>("txid")
                };
            }
        }

        private class WatcherInfo
        {
            public string Address { get; set; }

            public Func<IEnumerable<Transaction>, bool> Callback { get; set; }
        }
    }
}