using System.Text;
using Asp.Versioning;
using FoundationaLLM.Agent.Models.Metadata;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Agent.ResourceProviders;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing user profiles.
    /// </summary>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ClientBrandingConfiguration _settings;
        private readonly IResourceProviderService _agentResourceProvider;

        /// <summary>
        /// Constructor for the UserProfiles Controller.
        /// </summary>
        /// <param name="userProfileService">The Core service provides methods for managing the user profile.</param>
        /// <param name="settings">The branding configuration for the client.</param>
        public UserProfilesController(
            IUserProfileService userProfileService,
            IOptions<ClientBrandingConfiguration> settings,
            IEnumerable<IResourceProviderService> resourceProviderServices)
        {
            _userProfileService = userProfileService;
            var resourceProviderServicesDictionary = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
            if (!resourceProviderServicesDictionary.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            _agentResourceProvider = agentResourceProvider;
            _settings = settings.Value;
        }

        /// <summary>
        /// Retrieves the branding information for the client.
        /// </summary>
        [HttpGet(Name = "GetUserProfile")]
        public async Task<IActionResult> Index() =>
            Ok(await _userProfileService.GetUserProfileAsync());

        /// <summary>
        /// Retrieves a list of global and private agents.
        /// </summary>
        /// <returns></returns>
        [HttpGet("agents", Name = "GetAgents")]
        public async Task<IEnumerable<Common.Models.Metadata.Agent>> GetAgents()
        {
            var agents = new List<Common.Models.Metadata.Agent>();
            var globalAgentsList = await _agentResourceProvider.GetResourcesAsync<AgentReference>($"/{AgentResourceTypeNames.AgentReferences}");
            UserProfile? userProfile;

            try
            {
                userProfile = await _userProfileService.GetUserProfileAsync();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                userProfile = null;
            }
            
            if (globalAgentsList.Any())
            {
                agents.AddRange(globalAgentsList.Select(globalAgent => new Common.Models.Metadata.Agent {Name = globalAgent.Name, Private = false}));
            }

            if (userProfile?.PrivateAgents != null)
            {
                agents.AddRange(userProfile.PrivateAgents.Select(agent => new Common.Models.Metadata.Agent { Name = agent.Name, Private = true}));
            }

            return agents;
        }
    }
}
