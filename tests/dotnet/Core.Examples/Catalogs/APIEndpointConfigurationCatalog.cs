using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class APIEndpointConfigurationCatalog
    {
        public static readonly List<APIEndpointConfiguration> APIEndpointConfigurations =
        [
            new APIEndpointConfiguration
            {
                Name = TestAPIEndpointConfigurationNames.DefaultAzureOpenAI,
                Description = "The default Azure OpenAI endpoint.",
                Category = APIEndpointCategory.LLM,
                AuthenticationType = AuthenticationTypes.APIKey,
                Url = "FoundationaLLM:AzureOpenAI:API:Endpoint", // must be filled in during the test environment setup
                TimeoutSeconds = 1800,
                RetryStrategyName = TestRetryStrategyNames.ExponentialBackoff
            },
            new APIEndpointConfiguration
            {
                Name = TestAPIEndpointConfigurationNames.DefaultAzureAISearch,
                Description = "The default Azure AI Search endpoint.",
                Category = APIEndpointCategory.LLM,
                AuthenticationType = AuthenticationTypes.APIKey,
                Url = "FoundationaLLM:AzureAISearch:API:Endpoint", // must be filled in during the test environment setup
                TimeoutSeconds = 1800,
                RetryStrategyName = TestRetryStrategyNames.ExponentialBackoff
            }
        ];

        /// <summary>
        /// Retrieves all API endpoint configurations defined in the catalog.
        /// </summary>
        /// <returns></returns>
        public static List<APIEndpointConfiguration> GetAllAPIEndpointConfigurations()
        {
            var apiEndpointConfigurations = new List<APIEndpointConfiguration>();
            apiEndpointConfigurations.AddRange(APIEndpointConfigurations);

            return apiEndpointConfigurations;
        }
    }
}
