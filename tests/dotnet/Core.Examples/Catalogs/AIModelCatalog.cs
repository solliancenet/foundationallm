using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class AIModelCatalog
    {
        public static readonly List<CompletionAIModel> CompletionAIModels =
        [
            new CompletionAIModel {
                Name = TestAIModelNames.Completions_Default,
                Description = "The default completions AI model.",
                EndpointObjectId = TestAPIEndpointConfigurationNames.DefaultAzureOpenAI, // must be filled in during the test environment setup
                DeploymentName = TestAIModelNames.Completions_Default,
                ModelParameters = new Dictionary<string, object>
                {
                    { "temperature", 0 },
                }
            },
             new CompletionAIModel {
                Name = TestAIModelNames.Completions_GPT4_32K,
                Description = "The GPT-4 32K completions AI model.",
                EndpointObjectId = TestAPIEndpointConfigurationNames.DefaultAzureOpenAI, // must be filled in during the test environment setup
                DeploymentName = TestAIModelNames.Completions_GPT4_32K,
                ModelParameters = new Dictionary<string, object>
                {
                    { "temperature", 0 },
                }
            }
        ];

        public static readonly List<EmbeddingAIModel> EmbeddingAIModels =
        [
            new EmbeddingAIModel {
                    Name = TestAIModelNames.Embeddings_Default,
                    Description = "The default embeddings AI model.",
                    EndpointObjectId = TestAPIEndpointConfigurationNames.DefaultAzureOpenAI, // must be filled in during the test environment setup
                    DeploymentName = TestAIModelNames.Embeddings_Default
                }
        ];

        /// <summary>
        /// Retrieves all AI models defined in the catalog.
        /// </summary>
        /// <returns></returns>
        public static List<AIModelBase> GetAllAIModels()
        {
            var aiModels = new List<AIModelBase>();
            aiModels.AddRange(CompletionAIModels);
            aiModels.AddRange(EmbeddingAIModels);
            return aiModels;
        }
    }
}
