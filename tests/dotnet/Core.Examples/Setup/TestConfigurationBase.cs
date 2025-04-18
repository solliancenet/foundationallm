﻿using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Core.Examples.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace FoundationaLLM.Core.Examples.Setup
{
    /// <summary>
    /// Configures the test services for the examples.
    /// </summary>
    public class TestConfigurationBase
    {
        protected static TestConfigurationBase? _instance;
        protected static ConfigurationClient? _client;

        protected readonly IConfiguration _configuration;
        protected readonly ChainedTokenCredential _tokenCredential;

		public TestConfigurationBase(IConfiguration configuration)
        {
            _configuration = configuration;

            _tokenCredential = new(
                new AzureCliCredential(),
                new DefaultAzureCredential());
        }

        /// <summary>
        /// Initializes the test configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="services"></param>
        public static void Initialize(IConfiguration configuration, IServiceCollection services)
        {
            _instance = new TestConfigurationBase(configuration);

            var connectionString =
                _instance._configuration.GetValue<string>(EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString);
            _client = new ConfigurationClient(connectionString);
        }

        /// <summary>
        /// Gets the token credential for the test configuration.
        /// </summary>
        /// <returns></returns>
        public static TokenCredential GetTokenCredential()
        {
	        ValidateInstance();

	        return _instance!._tokenCredential;
        }

        /// <summary>
        /// Loads a section from the configuration. The source is typically the testsettings.json file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="caller"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ConfigurationNotFoundException"></exception>
        protected static T LoadSection<T>([CallerMemberName] string? caller = null)
        {
			ValidateInstance();

			if (string.IsNullOrEmpty(caller))
            {
                throw new ArgumentNullException(nameof(caller));
            }

            return _instance!._configuration.GetSection(caller).Get<T>() ??
                   throw new ConfigurationNotFoundException(section: caller);
        }

        /// <summary>
        /// Resolves a single App Config value.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string> GetAppConfigValueAsync(string key)
        {
			ValidateInstance();

			if (_client == null) return string.Empty;
			var response = await _client.GetConfigurationSettingAsync(key);

			if (response.Value is SecretReferenceConfigurationSetting secretReference)
			{
				var identifier = new KeyVaultSecretIdentifier(secretReference.SecretId);
				var secretClient = new SecretClient(identifier.VaultUri, _instance!._tokenCredential);
				var secret = await secretClient.GetSecretAsync(identifier.Name, identifier.Version);

				return secret.Value.Value;
			}

			return response.Value.Value;
        }

		/// <summary>
		/// Loads a section from App Config.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="keyFilter"></param>
		/// <returns></returns>
		public static async Task<T> GetAppConfigSectionAsync<T>(string keyFilter)
		{
			ValidateInstance();

			var selector = new SettingSelector { KeyFilter = keyFilter };

			var instance = Activator.CreateInstance<T>();

			if (_client == null) return instance;
			await foreach (var setting in _client.GetConfigurationSettingsAsync(selector))
			{
				string value;
				if (setting is SecretReferenceConfigurationSetting secretReference)
				{
					var identifier = new KeyVaultSecretIdentifier(secretReference.SecretId);
					var secretClient = new SecretClient(identifier.VaultUri, _instance!._tokenCredential);
					var secret = await secretClient.GetSecretAsync(identifier.Name, identifier.Version);

					value = secret.Value.Value;
				}
				else
				{
					value = setting.Value;
				}

				var propertyKey = setting.Key.Split(':').Last();
				var property = typeof(T).GetProperty(propertyKey);
				if (property != null && property.CanWrite)
				{
					object convertedValue;
					if (property.PropertyType.IsEnum)
					{
						convertedValue = Enum.Parse(property.PropertyType, value, ignoreCase: true);
					}
					else
					{
						convertedValue = Convert.ChangeType(value, property.PropertyType);
					}
					property.SetValue(instance, convertedValue);
				}
			}

			return instance;
		}

		private static void ValidateInstance()
        {
	        if (_instance == null)
	        {
		        throw new InvalidOperationException(
			        "TestConfiguration must be initialized with a call to Initialize(IConfigurationRoot) before accessing configuration values.");
	        }
        }

	}
}
