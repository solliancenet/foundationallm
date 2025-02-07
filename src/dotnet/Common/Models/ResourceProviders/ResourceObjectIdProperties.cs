using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Defines the properties of a resource.
    /// </summary>
    public class ResourceObjectIdProperties
    {
         /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        public required string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of properties.
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = [];

        /// <summary>
        /// Indicates whether the resource has the specified object role.
        /// </summary>
        /// <param name="role">The object role being searched.</param>
        /// <returns><see langword="true"/> if the object role is present, <see langword="false"/> otherwise.</returns>
        public bool HasObjectRole(string role)
        {
            if (Properties.TryGetValue(ResourceObjectIdPropertyNames.ObjectRole, out var objectRole))
            {
                return objectRole switch
                {
                    JsonElement element => element.GetString() == role,
                    _ => objectRole.ToString() == role
                };
            }
            return false;
        }
    }
}
