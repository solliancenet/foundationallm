using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage data source resources.
    /// </summary>
    public interface IDataSourceManagementClient
    {
        /// <summary>
        /// Retrieves all data source resources.
        /// </summary>
        /// <returns>All data source resources to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<DataSourceBase>>> GetDataSourcesAsync();

        /// <summary>
        /// Retrieves a specific data source by name.
        /// </summary>
        /// <param name="dataSourceName">The name of the data source resource to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<DataSourceBase>> GetDataSourceAsync(string dataSourceName);

        /// <summary>
        /// Checks the availability of a resource name for a data source. If the name is available, the
        /// <see cref="ResourceNameCheckResult.Status"/> value will be "Allowed". If the name is
        /// not available, the <see cref="ResourceNameCheckResult.Status"/> value will be "Denied" and
        /// the <see cref="ResourceNameCheckResult.Message"/> will explain the reason why. Typically,
        /// a denied name is due to a name conflict with an existing data source or a data source that was
        /// deleted but not purged.
        /// </summary>
        /// <param name="resourceName">Contains the name of the resource to check.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the required properties within the argument
        /// are empty or missing.</exception>
        Task<ResourceNameCheckResult> CheckDataSourceNameAsync(ResourceName resourceName);

        /// <summary>
        /// Purges a deleted data source by its name. This action is irreversible.
        /// </summary>
        /// <param name="dataSourceName">The name of the data source to purge.</param>
        /// <returns></returns>
        Task<ResourceProviderActionResult> PurgeDataSourceAsync(string dataSourceName);

        /// <summary>
        /// Returns data sources that match the filter criteria.
        /// </summary>
        /// <param name="resourceFilter">The filter criteria to apply to the request.</param>
        /// <returns></returns>
        Task<List<DataSourceBase>> FilterDataSourceAsync(ResourceFilter resourceFilter);

        /// <summary>
        /// Upserts a data source resource. If a data source does not exist, it will be created. If a data source
        /// does exist, it will be updated.
        /// </summary>
        /// <param name="dataSource">The data source resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertDataSourceAsync(DataSourceBase dataSource);

        /// <summary>
        /// Upserts a data source resource. If a data source does not exist, it will be created. If a data source
        /// does exist, it will be updated.
        /// </summary>
        /// <param name="dataSourceName">The data source resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task DeleteDataSourceAsync(string dataSourceName);
    }
}
