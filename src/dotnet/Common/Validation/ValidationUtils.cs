using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Diagnostics.CodeAnalysis;

namespace FoundationaLLM.Common.Validation
{
    /// <summary>
    /// Provides general purpose validation utilities.
    /// </summary>
    public class ValidationUtils
    {
        /// <summary>
        /// Validates the correctness of a FoundationaLLM object identifier.
        /// </summary>
        /// <param name="objectId">The FoundationaLLM object identifier to be validated.</param>
        /// <returns></returns>
        public static bool ValidateObjectId(string? objectId) =>
            !string.IsNullOrWhiteSpace(objectId)
            && ResourcePath.TryParse(
                objectId,
                ResourceProviderNames.All,
                ResourceProviderMetadata.AllAllowedResourceTypes,
                true,
                out _);

        /// <summary>
        /// Validates the correctness of a string representing a GUID value.
        /// </summary>
        /// <param name="guid">The string to be validated.</param>
        /// <returns></returns>
        public static bool ValidateGuid(string? guid) =>
            !string.IsNullOrWhiteSpace(guid)
            && Guid.TryParse(guid, out _);
    }
}
