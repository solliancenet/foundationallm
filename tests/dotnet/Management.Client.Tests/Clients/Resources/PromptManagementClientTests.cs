using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using NSubstitute;

namespace Management.Client.Tests.Clients.Resources
{
    public class PromptManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly PromptManagementClient _promptClient;

        public PromptManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _promptClient = new PromptManagementClient(_mockRestClient);
        }

        [Fact]
        public async Task GetPromptsAsync_ShouldReturnPrompts()
        {
            // Arrange
            var expectedPrompts = new List<ResourceProviderGetResult<PromptBase>>
            {
                new ResourceProviderGetResult<PromptBase>
                {
                    Resource = new MultipartPrompt()
                    {
                        Name = "agent-norman",
                        Prefix = "YOu are an analytic agent named Norman. You can answer questions about Norman Rockwell's life and work."
                    },
                    Actions = [],
                    Roles = []
                },
                new ResourceProviderGetResult<PromptBase>
                {
                    Resource = new MultipartPrompt()
                    {
                        Name = "agent-bernice",
                        Prefix = "YOu are an analytic agent named Bernice. You can answer questions about all duck breeds and what they eat."
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                    ResourceProviderNames.FoundationaLLM_Prompt,
                    PromptResourceTypeNames.Prompts
                )
                .Returns(Task.FromResult(expectedPrompts));

            // Act
            var result = await _promptClient.GetPromptsAsync();

            // Assert
            Assert.Equal(expectedPrompts, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                PromptResourceTypeNames.Prompts
            );
        }

        [Fact]
        public async Task GetPromptAsync_ShouldReturnPrompt()
        {
            // Arrange
            var promptName = "agent-bernice";
            var expectedPrompt = new ResourceProviderGetResult<PromptBase>
            {
                Resource = new MultipartPrompt()
                {
                    Name = promptName,
                    Prefix = "YOu are an analytic agent named Bernice. You can answer questions about all duck breeds and what they eat."
                },
                Actions = [],
                Roles = []
            };
            var expectedPrompts = new List<ResourceProviderGetResult<PromptBase>> { expectedPrompt };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                    ResourceProviderNames.FoundationaLLM_Prompt,
                    $"{PromptResourceTypeNames.Prompts}/{promptName}"
                )
                .Returns(Task.FromResult(expectedPrompts));

            // Act
            var result = await _promptClient.GetPromptAsync(promptName);

            // Assert
            Assert.Equal(expectedPrompt, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{promptName}"
            );
        }

        [Fact]
        public async Task GetPromptAsync_ShouldThrowException_WhenPromptNotFound()
        {
            // Arrange
            var promptName = "test-prompt";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<PromptBase>>>(
                    ResourceProviderNames.FoundationaLLM_Prompt,
                    $"{PromptResourceTypeNames.Prompts}/{promptName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<PromptBase>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _promptClient.GetPromptAsync(promptName));
            Assert.Equal($"Prompt '{promptName}' not found.", exception.Message);
        }

        [Fact]
        public async Task CheckPromptNameAsync_ShouldReturnCheckResult()
        {
            // Arrange
            var resourceName = new ResourceName { Name = "test-prompt", Type = "prompt-type" };
            var expectedCheckResult = new ResourceNameCheckResult
            {
                Name = resourceName.Name,
                Status = NameCheckResultType.Allowed,
                Message = "Name is allowed"
            };

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceNameCheckResult>(
                    ResourceProviderNames.FoundationaLLM_Prompt,
                    $"{PromptResourceTypeNames.Prompts}/{PromptResourceProviderActions.CheckName}",
                    resourceName
                )
                .Returns(Task.FromResult(expectedCheckResult));

            // Act
            var result = await _promptClient.CheckPromptNameAsync(resourceName);

            // Assert
            Assert.Equal(expectedCheckResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{PromptResourceProviderActions.CheckName}",
                resourceName
            );
        }

        [Fact]
        public async Task CheckPromptNameAsync_ShouldThrowArgumentException_WhenResourceNameIsInvalid()
        {
            // Arrange
            var invalidResourceName = new ResourceName { Name = "", Type = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _promptClient.CheckPromptNameAsync(invalidResourceName));
        }

        [Fact]
        public async Task PurgePromptAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var promptName = "test-prompt";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_Prompt,
                    $"{PromptResourceTypeNames.Prompts}/{promptName}/{PromptResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _promptClient.PurgePromptAsync(promptName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{promptName}/{PromptResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgePromptAsync_ShouldThrowArgumentException_WhenPromptNameIsInvalid()
        {
            // Arrange
            var invalidPromptName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _promptClient.PurgePromptAsync(invalidPromptName));
        }

        [Fact]
        public async Task UpsertPromptAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var prompt = new PromptBase { Name = "test-prompt" };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Prompt,
                    $"{PromptResourceTypeNames.Prompts}/{prompt.Name}",
                    prompt
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _promptClient.UpsertPromptAsync(prompt);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{prompt.Name}",
                prompt
            );
        }

        [Fact]
        public async Task DeletePromptAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var promptName = "test-prompt";

            // Act
            await _promptClient.DeletePromptAsync(promptName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"{PromptResourceTypeNames.Prompts}/{promptName}"
            );
        }
    }
}
