using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatekeeper.Tests.Services
{
    public class GatekeeperServiceTests
    {
        [Fact]
        public async Task GetCompletion_CallsAgentFactoryAPIServiceWithCompletionRequest()
        {
            // Arrange
            var agentFactoryAPIService = Substitute.For<IAgentFactoryAPIService>();
            var refinementService = Substitute.For<IRefinementService>();
            var contentSafetyService = Substitute.For<IContentSafetyService>();
            var service = new GatekeeperService(agentFactoryAPIService, refinementService, contentSafetyService);
            var completionRequest = new CompletionRequest { UserPrompt = "Prompt_1", MessageHistory = new List<FoundationaLLM.Common.Models.Chat.MessageHistoryItem>() };

            // Act
            await service.GetCompletion(completionRequest);

            // Assert
            await agentFactoryAPIService.Received(1).GetCompletion(Arg.Is(completionRequest));
        }

        [Fact]
        public async Task GetSummary_CallsAgentFactoryAPIServiceWithSummaryRequest()
        {
            // Arrange
            var agentFactoryAPIService = Substitute.For<IAgentFactoryAPIService>();
            var refinementService = Substitute.For<IRefinementService>();
            var contentSafetyService = Substitute.For<IContentSafetyService>();
            var service = new GatekeeperService(agentFactoryAPIService, refinementService, contentSafetyService);
            var summaryRequest = new SummaryRequest { UserPrompt = "Prompt_1" };

            // Act
            await service.GetSummary(summaryRequest);

            // Assert
            await agentFactoryAPIService.Received(1).GetSummary(Arg.Is(summaryRequest));
        }
    }
}
