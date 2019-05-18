using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using BitcoinPOS_App.Interfaces.Providers;
using BitcoinPOS_App.Providers;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(EssentialSettingsProvider))]

namespace BitcoinPOS_App.Providers
{
    public class EssentialSettingsProvider : ISettingsProvider
    {
        private static readonly Dictionary<Type, MethodInfo> GetCrossSettingsMethods;
        private static readonly Dictionary<Type, MethodInfo> SetCrossSettingsMethods;

        static EssentialSettingsProvider()
        {
            Dictionary<Type, MethodInfo> GetCrossSettingsOverloads(string overloadNames)
            {
                return typeof(ISettings)
                    .GetMethods()
                    .Select(m => (method: m, parameters: m.GetParameters()))
                    .Where(m => m.method.Name == overloadNames && m.parameters.Length == 3)
                    .ToDictionary(d => d.parameters[1].ParameterType, d => d.method);
            }

            GetCrossSettingsMethods = GetCrossSettingsOverloads(nameof(ISettings.GetValueOrDefault));
            SetCrossSettingsMethods = GetCrossSettingsOverloads(nameof(ISettings.AddOrUpdateValue));
        }

        public async Task<T> GetSecureValueAsync<T>(string key)
        {
            CheckKey(key);
            string secureValue;
            try
            {
                secureValue = await SecureStorage.GetAsync(key);
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERRO: Falha ao buscar config segura. {0}", e);
                secureValue = null;
            }

            if (string.IsNullOrWhiteSpace(secureValue))
                return default;

            using (var ms = new MemoryStream(Convert.FromBase64String(secureValue)))
            {
                return (T) new BinaryFormatter()
                    .Deserialize(ms);
            }
        }

        public Task<T> GetValueAsync<T>(string key)
        {
            CheckKey(key);

            var value = (T) GetCrossSettingsMethods[typeof(T)]
                .Invoke(CrossSettings.Current, new object[] {key, default(T), null});

            return Task.FromResult(value);
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

            SetCrossSettingsMethods[typeof(T)]
                .Invoke(CrossSettings.Current, new object[] {key, value, null});

            return Task.CompletedTask;
        }

        private static void CheckKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("The key cannot be null or empty.", nameof(key));
        }
    }
}