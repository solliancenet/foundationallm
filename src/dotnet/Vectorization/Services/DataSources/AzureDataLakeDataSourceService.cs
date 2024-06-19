using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.DataSources
{
    /// <summary>
    /// Provides access to Azure Data Lake based on a data source.
    /// </summary>
    public class AzureDataLakeDataSourceService
    {
        private readonly ILogger<AzureDataLakeDataSourceService> _logger;        
        private readonly AzureDataLakeDataSource _dataSource;
        private readonly DataLakeStorageService _dataLakeStorageService;
        private readonly BlobStorageServiceSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureDataLakeDataSourceService"/> class.
        /// </summary>        
        /// <param name="dataSource">The data source definition.</param>
        /// <param name="settings">Blob storage configuration settings.</param>
        /// <param name="loggerFactory">The factory responsible for creating loggers.</param>
        public AzureDataLakeDataSourceService(            
            AzureDataLakeDataSource dataSource,
            BlobStorageServiceSettings settings,
            ILoggerFactory loggerFactory)
        {
            
            _dataSource = dataSource;
            _logger = loggerFactory.CreateLogger<AzureDataLakeDataSourceService>();
            _settings = settings;
           
            _dataLakeStorageService = new DataLakeStorageService(
                               _settings,
                               loggerFactory.CreateLogger<DataLakeStorageService>());
        }

        /// <summary>
        /// Gets the list of files in the data source folders
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetFilesListAsync()
        {
            List<string> files = new List<string>();
            foreach (var folder in _dataSource.Folders)
            {
                //trim any preceding or trailing slashes
                var trimmedFolder = folder.Trim('/');
                //the first token is the container or workspace name
                var container = trimmedFolder.Split('/')[0];
                //remove the first token from the path
                var path = trimmedFolder.Substring(trimmedFolder.IndexOf('/') + 1);

                if(trimmedFolder.Equals(container) && container.Equals(path))
                {
                    //artifacts are in the root of the container
                    path = string.Empty;
                }

                var filesList = await _dataLakeStorageService!.GetFilePathsAsync(container, path);
                //pre-pend container name to the file path for each string in filesList
                files.AddRange(filesList.Select(f => $"{container}/{f}"));
            }
            return files;
        }
    }

}
