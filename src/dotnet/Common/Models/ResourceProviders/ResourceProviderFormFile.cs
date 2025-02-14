namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides details about a file uploaded through the IFormFile mechanism from ASP.NET.
    /// </summary>
    public class ResourceProviderFormFile
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public required string FileName { get; set; }

        /// <summary>
        /// The mime content type of the file.
        /// </summary>
        public required string ContentType { get; set; }

        /// <summary>
        /// The binary content of the file.
        /// </summary>
        public required ReadOnlyMemory<byte> BinaryContent { get; set; }

        /// <summary>
        /// Additional optional parameters from the form payload.
        /// </summary>
        public Dictionary<string, string>? Payload { get; set; }
    }
}
