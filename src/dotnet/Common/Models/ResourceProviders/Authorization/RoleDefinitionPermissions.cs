using System.Text.Json.Serialization;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Authorization
{
    /// <summary>
    /// Manages the permissions associated with a security role definition.
    /// </summary>
    public class RoleDefinitionPermissions
    {
        /// <summary>
        /// Allowed control plane actions.
        /// </summary>
        [JsonPropertyName("actions")]
        [JsonPropertyOrder(1)]
        public List<string> Actions { get; set; } = [];

        /// <summary>
        /// Denied control plane actions.
        /// </summary>
        [JsonPropertyName("not_actions")]
        [JsonPropertyOrder(2)]
        public List<string> NotActions { get; set; } = [];

        /// <summary>
        /// Allowed data plane actions.
        /// </summary>
        [JsonPropertyName("data_actions")]
        [JsonPropertyOrder(3)]
        public List<string> DataActions { get; set; } = [];

        /// <summary>
        /// Denied data plane actions.
        /// </summary>
        [JsonPropertyName("not_data_actions")]
        [JsonPropertyOrder(4)]
        public List<string> NotDataActions { get; set; } = [];

        public List<string> GetAllowedActions()
        {
            var actions = Actions
                .SelectMany(x => AuthorizableActions.GetMatchingActions(x))
                .Distinct()
                .ToList();

            var notActions = NotActions
                .SelectMany(x => AuthorizableActions.GetMatchingActions(x))
                .Distinct()
                .ToList();

            var dataActions = DataActions
                .SelectMany(x => AuthorizableActions.GetMatchingActions(x))
                .Distinct()
                .ToList();

            var notDataActions = NotDataActions
                .SelectMany(x => AuthorizableActions.GetMatchingActions(x))
                .Distinct()
                .ToList();

            return actions
                .Except(notActions)
                .Union(dataActions)
                .Except(notDataActions)
                .ToList();
        }
    }
}
