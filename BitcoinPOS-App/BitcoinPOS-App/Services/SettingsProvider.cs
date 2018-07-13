using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(BitcoinPOS_App.Services.EssentialSettingsProvider))]

namespace BitcoinPOS_App.Services
{
    public class EssentialSettingsProvider : ISettingsProvider
    {
        public async Task<T> GetSecureValueAsync<T>(string key)
        {
            CheckKey(key);

            var secureValue = await SecureStorage.GetAsync(key);

            using (var ms = new MemoryStream(Convert.FromBase64String(secureValue)))
            {
                return (T)new BinaryFormatter()
                    .Deserialize(ms);
            }
        }

        public Task<T> GetValueAsync<T>(string key)
        {
            CheckKey(key);
            throw new NotImplementedException();
        }

        public Task SetSecureValueAsync<T>(string key, T value)
        {
            CheckKey(key);

            using (var ms = new MemoryStream())
            {
                new BinaryFormatter()
                    .Serialize(ms, value);

                ms.Position = 0L;

                return SecureStorage.SetAsync(key, Convert.ToBase64String(ms.ToArray()));
            }
        }

        public Task SetValueAsync<T>(string key, T value)
        {
            CheckKey(key);
            throw new NotImplementedException();
        }

        private static void CheckKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("The key cannot be null or empty.", nameof(key));
        }
    }
}
