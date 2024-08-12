namespace FoundationaLLM.Common.Models.Configuration.Instance
{
    /// <summary>
    /// Provides configuration settings for the current FoundationaLLM deployment instance.
    /// </summary>
    public class InstanceSettings
    {
        /// <summary>
        /// The unique identifier of the current FoundationaLLM deployment instance.
        /// Format is a GUID.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The security group retrieval strategy of the FoundationaLLM instance.
        /// </summary>
        public string? SecurityGroupRetrievalStrategy { get; set; }

        /// <summary>
        /// The FoundationaLLM Version.
        /// </summary>
        public required string Version { get; set; }
    }
}
