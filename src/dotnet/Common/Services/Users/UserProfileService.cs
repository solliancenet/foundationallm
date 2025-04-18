﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Users;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Users
{
    /// <inheritdoc/>
    public class UserProfileService : IUserProfileService
    {
        private readonly IAzureCosmosDBService _cosmosDbService;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IOrchestrationContext _callContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreService"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The Azure Cosmos DB service that contains
        /// user profiles.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="UserProfileService"/> type name.</param>
        /// <param name="callContext">Contains contextual data for the calling service.</param>
        public UserProfileService(IAzureCosmosDBService cosmosDbService,
            ILogger<UserProfileService> logger,
            IOrchestrationContext callContext)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
            _callContext = callContext;
        }

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileAsync(string instanceId) => await _cosmosDbService.GetUserProfileAsync(_callContext.CurrentUserIdentity?.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving the user profile."));

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileForUserAsync(string instanceId, string upn) =>
            await _cosmosDbService.GetUserProfileAsync(upn);

        /// <inheritdoc/>
        public async Task UpsertUserProfileAsync(string instanceId, UserProfile userProfile)
        {
            // Ensure the user profile contains the user's UPN.
            if (string.IsNullOrEmpty(userProfile.UPN))
            {
                userProfile.UPN = _callContext.CurrentUserIdentity?.UPN
                    ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat sessions.");
                userProfile.Id = userProfile.UPN;
            }
            await _cosmosDbService.UpsertUserProfileAsync(userProfile);
        }
    }
}
