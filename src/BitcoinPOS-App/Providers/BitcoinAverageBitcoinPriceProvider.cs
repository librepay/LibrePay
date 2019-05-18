using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Models;
using BitcoinPOS_App.Providers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Caching.Memory;
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
        private static readonly Context LocalPriceContext = new Context("local-price");

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

            var policyBuilder = Policy.HandleResult<HttpResponseMessage>(h => !h.IsSuccessStatusCode)
                .Or<Exception>();

            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
            var memoryCacheProvider = new MemoryCacheProvider(memoryCache);

            DefaultPolicy = Policy.WrapAsync(
                Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30))
                , Policy.CacheAsync<HttpResponseMessage>(
                    memoryCacheProvider
                    , TimeSpan.FromMinutes(Constants.MinutesBetweenExchangePriceChecks)
                    , (context, key, ex) => Debugger.Break()
                )
                , policyBuilder.WaitAndRetryForeverAsync(
                    sleepDurationProvider: i => TimeSpan.FromSeconds(3)
                    , onRetry: (r, _, i) => Debug.WriteLine(
                        "[INFO] Tentando novamente chamada em BitcoinAverage." +
                        $"\nErro anterior: {r.Exception}" +
                        $"\nResultado anterior: {r.Result?.StatusCode}"
                    )
                )
                , policyBuilder
                    .CircuitBreakerAsync(1, TimeSpan.FromSeconds(5))
                , Policy.BulkheadAsync<HttpResponseMessage>(1)
                , Policy.TimeoutAsync<HttpResponseMessage>(() => TimeSpan.FromSeconds(15))
            ).WithPolicyKey("bitcoin-average");
        }

        public async Task<ExchangeRate> GetLocalBitcoinPrice()
        {
            var response = await DefaultPolicy
                .ExecuteAsync(ExecuteRequest, LocalPriceContext);

            var rawBody = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(rawBody);

            var price = json["averages"].Value<decimal>("day");
            var date = json.Value<DateTime>("display_timestamp");

            Debug.WriteLine($"[INFO] Obteu valor de troca: {price}");

            return new ExchangeRate(price, "R$/BTC", date);
        }

        private Task<HttpResponseMessage> ExecuteRequest(Context _)
        {
            var ri = new RegionInfo(Thread.CurrentThread.CurrentUICulture.LCID);
            Debug.WriteLine($"Buscando preço médio do dia no BitcoinAverage BTC <=> {ri.ISOCurrencySymbol}", "INFO");

            // !!! Currency symbol must be set on the Settings page
            // !!! Just getting the symbol automaticaly is not acceptable
            // !!! because user can have phone set on one region while actually being in another place
            //return HttpClient.GetAsync($"/indices/local/ticker/BTC{ri.ISOCurrencySymbol}");

            return HttpClient.GetAsync($"/indices/local/ticker/BTCBRL");
        }
    }
}
