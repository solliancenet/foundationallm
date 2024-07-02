using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class DataSourceCatalog
    {
        public static readonly List<DataSourceBase> Items =
        [
            new AzureDataLakeDataSource { Name = "datalake_different_file_types", DisplayName = "datalake_different_file_types", ConfigurationReferences = new Dictionary<string, string> { { "AuthenticationType", "FoundationaLLM:DataSources:datalake_different_file_types:AuthenticationType" }, { "AccountName", "FoundationaLLM:DataSources:datalake_different_file_types:AccountName" } }, Folders = new List<string> { "vectorization-input/fllm-org-data" } },
            new AzureDataLakeDataSource { Name = "datalake_vectorization_input", DisplayName = "datalake_vectorization_input", ConfigurationReferences = new Dictionary<string, string> { { "AuthenticationType", "FoundationaLLM:DataSources:datalake_vectorization_input:AuthenticationType" }, { "AccountName", "FoundationaLLM:DataSources:datalake_vectorization_input:AccountName" } }, Folders = new List<string> { "vectorization-input" } },
            new AzureDataLakeDataSource { Name = "onelake_fllm", DisplayName = "onelake_fllm", ConfigurationReferences = new Dictionary<string, string> { { "AuthenticationType", "FoundationaLLM:DataSources:onelake_fllm:AuthenticationType" }, { "AccountName", "FoundationaLLM:DataSources:onelake_fllm:AccountName" } }, Folders = new List<string> { "FoundationaLLM" } },
            new SharePointOnlineSiteDataSource { Name = "sharepoint_fllm", DisplayName="sharepoint_fllm", SiteUrl="https://fllm.sharepoint.com/sites/FoundationaLLM", DocumentLibraries=["/documents02"], ConfigurationReferences = new Dictionary<string, string>{ {"ClientId", "FoundationaLLM:DataSources:sharepoint_fllm:ClientId" },{"TenantId", "FoundationaLLM:DataSources:sharepoint_fllm:TenantId" },{"CertificateName", "FoundationaLLM:DataSources:sharepoint_fllm:CertificateName" },{ "KeyVaultURL", "FoundationaLLM:DataSources:sharepoint_fllm:KeyVaultURL" } } }
        ];

        public static List<DataSourceBase> GetDataSources()
        {
            var items = new List<DataSourceBase>();
            items.AddRange(Items);
            return items;
        }
    }
}
