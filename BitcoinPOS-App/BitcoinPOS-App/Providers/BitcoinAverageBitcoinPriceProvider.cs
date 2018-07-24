using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.Providers;
using Newtonsoft.Json.Linq;
using Polly;
using Xamarin.Forms;

[assembly: Dependency(typeof(BitcoinAverageBitcoinPriceProvider))]

namespace BitcoinPOS_App.Providers
{
    /// <summary>
    /// Info provider for https://bitcoinaverage.com/en/bitcoin-price/btc-to-brl
    /// https://apiv2.bitcoinaverage.com/#ticker-data-per-symbol
    /// </summary>
    public class BitcoinAverageBitcoinPriceProvider : IBitcoinPriceProvider
    {
        private static readonly HttpClient HttpClient;
        private static readonly Policy<HttpResponseMessage> DefaultPolicy;

        static BitcoinAverageBitcoinPriceProvider()
        {
            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://apiv2.bitcoinaverage.com"),
                DefaultRequestHeaders =
                {
                    Accept = {new MediaTypeWithQualityHeaderValue("application/json")}
                },
                Timeout = TimeSpan.FromSeconds(15)
            };

            DefaultPolicy = Policy.HandleResult<HttpResponseMessage>(h => !h.IsSuccessStatusCode)
                .Or<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3
                    , sleepDurationProvider: i => TimeSpan.FromSeconds(3)
                    , onRetry: (r, _, i) => Debug.WriteLine("[INFO] Tentando novamente chamada em BitcoinAverage." +
                                                            $"\nErro anterior: {r.Exception}" +
                                                            $"\nResultado anterior: {r.Result?.StatusCode}")
                );
        }

        public async Task<ExchangeRate> GetLocalBitcoinPrice()
        {
            var response = await DefaultPolicy
                .ExecuteAsync(() => HttpClient.GetAsync("/indices/local/ticker/BTCBRL"));

            var rawBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(rawBody);

            var price = json["averages"].Value<decimal>("day");
            var date = json.Value<DateTime>("display_timestamp");

            Debug.WriteLine($"[INFO] Obteu valor de troca: {price}");

            return new ExchangeRate(price, date);
        }
    }
}