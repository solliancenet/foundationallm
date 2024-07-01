using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using NSubstitute;

namespace Management.Client.Tests.Clients.Resources
{
    public class AgentManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly AgentManagementClient _agentClient;

        public AgentManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _agentClient = new AgentManagementClient(_mockRestClient);
        }

        [Fact]
        public async Task GetAgentsAsync_ShouldReturnAgents()
        {
            // Arrange
            var expectedAgents = new List<ResourceProviderGetResult<AgentBase>>
            {
                new() {
                    Resource = new AgentBase
                    {
                        Name = "test-agent",
                        Description = "A test agent",
                        Type = AgentTypes.KnowledgeManagement
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                    ResourceProviderNames.FoundationaLLM_Agent,
                    AgentResourceTypeNames.Agents
                )
                .Returns(Task.FromResult(expectedAgents));

            // Act
            var result = await _agentClient.GetAgentsAsync();

            // Assert
            Assert.Equal(expectedAgents, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                ResourceProviderNames.FoundationaLLM_Agent,
                AgentResourceTypeNames.Agents
            );
        }

        [Fact]
        public async Task GetAgentAsync_ShouldReturnAgent()
        {
            // Arrange
            var agentName = "test-agent";
            var expectedAgent = new ResourceProviderGetResult<AgentBase>
            {
                Resource = new AgentBase
                {
                    Name = agentName,
                    Description = "A test agent",
                    Type = AgentTypes.KnowledgeManagement
                },
                Actions = [],
                Roles = []
            };
            var expectedAgents = new List<ResourceProviderGetResult<AgentBase>> { expectedAgent };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                    ResourceProviderNames.FoundationaLLM_Agent,
                    $"{AgentResourceTypeNames.Agents}/{agentName}"
                )
                .Returns(Task.FromResult(expectedAgents));

            // Act
            var result = await _agentClient.GetAgentAsync(agentName);

            // Assert
            Assert.Equal(expectedAgent, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agentName}"
            );
        }

        [Fact]
        public async Task GetAgentAsync_ShouldThrowException_WhenAgentNotFound()
        {
            // Arrange
            var agentName = "test-agent";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AgentBase>>>(
                    ResourceProviderNames.FoundationaLLM_Agent,
                    $"{AgentResourceTypeNames.Agents}/{agentName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<AgentBase>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _agentClient.GetAgentAsync(agentName));
            Assert.Equal($"Agent '{agentName}' not found.", exception.Message);
        }

        [Fact]
        public async Task CheckAgentNameAsync_ShouldReturnCheckResult()
        {
            // Arrange
            var resourceName = new ResourceName { Name = "test-agent", Type = "agent-type" };
            var expectedCheckResult = new ResourceNameCheckResult
            {
                Name = resourceName.Name,
                Status = NameCheckResultType.Allowed,
                Message = "Name is allowed"
            };

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceNameCheckResult>(
                    ResourceProviderNames.FoundationaLLM_Agent,
                    $"{AgentResourceTypeNames.Agents}/{AgentResourceProviderActions.CheckName}",
                    resourceName
                )
                .Returns(Task.FromResult(expectedCheckResult));

            // Act
            var result = await _agentClient.CheckAgentNameAsync(resourceName);

            // Assert
            Assert.Equal(expectedCheckResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{AgentResourceProviderActions.CheckName}",
                resourceName
            );
        }

        [Fact]
        public async Task CheckAgentNameAsync_ShouldThrowArgumentException_WhenResourceNameIsInvalid()
        {
            // Arrange
            var invalidResourceName = new ResourceName { Name = "", Type = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _agentClient.CheckAgentNameAsync(invalidResourceName));
        }

        [Fact]
        public async Task PurgeAgentAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var agentName = "test-agent";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_Agent,
                    $"{AgentResourceTypeNames.Agents}/{agentName}/{AgentResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _agentClient.PurgeAgentAsync(agentName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agentName}/{AgentResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgeAgentAsync_ShouldThrowArgumentException_WhenAgentNameIsInvalid()
        {
            // Arrange
            var invalidAgentName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _agentClient.PurgeAgentAsync(invalidAgentName));
        }

        [Fact]
        public async Task UpsertAgentAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var agent = new AgentBase { Name = "test-agent" };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Agent,
                    $"{AgentResourceTypeNames.Agents}/{agent.Name}",
                    agent
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _agentClient.UpsertAgentAsync(agent);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agent.Name}",
                agent
            );
        }

        [Fact]
        public async Task DeleteAgentAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var agentName = "test-agent";

            // Act
            await _agentClient.DeleteAgentAsync(agentName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Agent,
                $"{AgentResourceTypeNames.Agents}/{agentName}"
            );
        }
    }
}
