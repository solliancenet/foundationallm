using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class DataSourceManagementClient(IManagementRESTClient managementRestClient) : IDataSourceManagementClient
    {
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<DataSourceBase>>> GetDataSourcesAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                DataSourceResourceTypeNames.DataSources
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<DataSourceBase>> GetDataSourceAsync(string dataSourceName)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"DataSource '{dataSourceName}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<ResourceNameCheckResult> CheckDataSourceNameAsync(ResourceName resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName.Name) || string.IsNullOrWhiteSpace(resourceName.Type))
            {
                throw new ArgumentException("Resource name and type must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{DataSourceResourceProviderActions.CheckName}",
                resourceName
            );
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderActionResult> PurgeDataSourceAsync(string dataSourceName)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName))
            {
                throw new ArgumentException("The DataSource name must be provided.");
            }

            return await managementRestClient.Resources.ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}/{DataSourceResourceProviderActions.Purge}",
                new { }
            );
        }

        /// <inheritdoc/>
        public async Task<List<DataSourceBase>> FilterDataSourceAsync(ResourceFilter resourceFilter) =>
            await managementRestClient.Resources.ExecuteResourceActionAsync<List<DataSourceBase>>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{DataSourceResourceProviderActions.Filter}",
                resourceFilter
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertDataSourceAsync(DataSourceBase dataSource) => await managementRestClient.Resources.UpsertResourceAsync(
            ResourceProviderNames.FoundationaLLM_DataSource,
            $"{DataSourceResourceTypeNames.DataSources}/{dataSource.Name}",
                dataSource
            );

        /// <inheritdoc/>
        public async Task DeleteDataSourceAsync(string dataSourceName) => await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}"
            );
    }
}
