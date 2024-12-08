namespace FoundationaLLM.AuthorizationEngine.Models
{
    /// <summary>
    /// Represents a key to be shared to or received from the client.  This key can be validated
    /// by fetching the corresponding IApiKeyStorage object and recomputing the hash using the
    /// ClientSecret + Salt as input. 
    /// </summary>
    public class ClientSecretKey
    {
        /// <summary>
        /// The unique identifier of this API key.
        /// </summary>
        public Guid ApiKeyId { get; set; }

        /// <summary>
        /// The client secret portion of this key.  This is information only supposed to be held in
        /// memory for a short period of time and delivered to the customer as quickly as possible.
        /// </summary>
        public required string ClientSecret { get; set; }

        /// <summary>
        /// Converts this API key into a string the customer can use for authentication.
        /// </summary>
        /// <param name="contextId">The context identifier.</param>
        /// <returns></returns>
        public string ToApiKeyString(string contextId)
        {
            var idBytes = ApiKeyId.ToByteArray();
            var idString = Base58.Encode(idBytes);
            var agentName = contextId.Split("~").Last();
            return $"keya.{agentName}.{idString}.{ClientSecret}.ayek";
        }
    }
}
