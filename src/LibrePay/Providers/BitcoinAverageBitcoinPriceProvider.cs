using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LibrePay.Interfaces.Providers;
using LibrePay.Models;
using LibrePay.Providers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Caching.Memory;
using Xamarin.Forms;

[assembly: Dependency(typeof(BitcoinAverageBitcoinPriceProvider))]

namespace LibrePay.Providers
{
    /// <summary>
    /// Info provider for https://bitcoinaverage.com/en/bitcoin-price/btc-to-brl
    /// https://apiv2.bitcoinaverage.com/#ticker-data-per-symbol
    /// </summary>
    public class BitcoinAverageBitcoinPriceProvider : IBitcoinPriceProvider
    {
        private static readonly HttpClient HttpClient;
        private static readonly AsyncPolicy<HttpResponseMessage> DefaultPolicy;
        private static readonly Context LocalPriceContext = new Context("local-price");

        private readonly CultureInfo _cultureInfo;
        private readonly RegionInfo _regionInfo;

        public BitcoinAverageBitcoinPriceProvider(
            CultureInfo cultureInfo
        )
        {
            _cultureInfo = cultureInfo;
            _regionInfo = new RegionInfo(_cultureInfo.LCID);
        }

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
                        "[INFO] Trying new call to BitcoinAverage." +
                        $"\nPrevious error: {r.Exception}" +
                        $"\nPrevious result: {r.Result?.StatusCode}"
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

            Debug.WriteLine($"[INFO] Got exchange rate: {price}");

            return new ExchangeRate(price, $"{_regionInfo.CurrencySymbol}/BTC", date, _cultureInfo);
        }

        private Task<HttpResponseMessage> ExecuteRequest(Context _)
        {
            Debug.WriteLine($"Getting daily average rate from BitcoinAverage BTC <=> {_regionInfo.ISOCurrencySymbol}", "INFO");

            // !!! Currency symbol must be set on the Settings page
            // !!! Just getting the symbol automaticaly is not acceptable
            // !!! because user can have phone set on one region while actually being in another place
			//TODO: Fix this with a user prompt? maybe is more of a UX problem
            return HttpClient.GetAsync($"/indices/local/ticker/BTC{_regionInfo.ISOCurrencySymbol}");
        }
    }
}
