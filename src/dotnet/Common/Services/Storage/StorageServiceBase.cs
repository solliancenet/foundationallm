﻿using FoundationaLLM.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Constants.Authentication;
using Azure.Storage.Blobs;

namespace FoundationaLLM.Common.Services.Storage
{
    /// <summary>
    /// Provides access to storage.
    /// </summary>
    public abstract class StorageServiceBase
    {
        /// <summary>
        /// The <see cref="BlobStorageServiceSettings"/> used to configure the storage service.
        /// </summary>
        protected readonly BlobStorageServiceSettings _settings;
        /// <summary>
        /// The logger used for logging.
        /// </summary>
        protected readonly ILogger<StorageServiceBase> _logger;

        /// <summary>
        /// The optional instance name of the storage service.
        /// </summary>
        public string? InstanceName { get; set; }

        /// <summary>
        /// The name of the storage account.
        /// </summary>
        public string? StorageAccountName => _settings.AccountName;

        /// <inheritdoc/>
        public string? StorageContainerName => _settings.ContainerName;

        /// <summary>
        ///  Initializes a new instance of the <see cref="StorageServiceBase"/> with the specified options and logger.
        /// </summary>
        /// <param name="storageOptions">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the settings.</param>
        /// <param name="logger">The logger used for logging.</param>
        public StorageServiceBase(
            IOptions<BlobStorageServiceSettings> storageOptions,
            ILogger<StorageServiceBase> logger)
        {
            _settings = storageOptions.Value;
            _logger = logger;

            Initialize();
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="StorageServiceBase"/> with the specified settings and logger.
        /// </summary>
        /// <param name="settings">The <see cref="BlobStorageServiceSettings"/> object with the settings.</param>
        /// <param name="logger">The logger used for logging.</param>
        public StorageServiceBase(
            BlobStorageServiceSettings settings,
            ILogger<StorageServiceBase> logger)
        {
            _settings = settings;
            _logger = logger;

            Initialize();
        }

        /// <summary>
        /// Creates a storage client from a connection string.
        /// </summary>
        /// <param name="connectionString">The storage connection string.</param>
        protected abstract void CreateClientFromConnectionString(string connectionString);

        /// <summary>
        /// Creates a storage client from an account name and an account key.
        /// </summary>
        /// <param name="accountName">The storage account name.</param>
        /// <param name="accountKey">The storage account key.</param>
        protected abstract void CreateClientFromAccountKey(string accountName, string accountKey);

        /// <summary>
        /// Create a storage client from an account name using the default identity for authentication.
        /// </summary>
        /// <param name="accountName">The storage account name.</param>
        protected abstract void CreateClientFromIdentity(string accountName);

        private void ValidateAccountName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure blob storage account name is invalid.");
                throw new ConfigurationValueException("The Azure blob storage account name is invalid.");
            }
        }

        private void ValidateAccountKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure blob storage account key is invalid.");
                throw new ConfigurationValueException("The Azure blob storage account key is invalid.");
            }
        }

        private void ValidateConnectionString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure blob storage account connection string is invalid.");
                throw new ConfigurationValueException("The Azure blob storage account connection string is invalid.");
            }
        }

        private void Initialize()
        {
            switch (_settings.AuthenticationType)
            {
                case AuthenticationTypes.ConnectionString:
                    ValidateConnectionString(_settings.ConnectionString);
                    CreateClientFromConnectionString(_settings.ConnectionString!);
                    break;
                case AuthenticationTypes.AccountKey:
                    ValidateAccountName(_settings.AccountName);
                    ValidateAccountKey(_settings.AccountKey);
                    CreateClientFromAccountKey(_settings.AccountName!, _settings.AccountKey!);
                    break;
                case AuthenticationTypes.AzureIdentity:
                    ValidateAccountName(_settings.AccountName);
                    CreateClientFromIdentity(_settings.AccountName!);
                    break;
                default:
                    throw new InvalidEnumArgumentException($"The authentication type {_settings.AuthenticationType} is not supported.");
            }
        }
    }
}
