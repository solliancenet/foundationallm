namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Common Swagger strings used throughout the FoundationaLLM infrastructure.
    /// </summary>
    public static class Swagger
    {
        /// <summary>
        /// The OpenAPI security definition name.
        /// </summary>
        public const string SecurityDefinitionName = "ApiKey";

        /// <summary>
        /// The OpenAPI security scheme name.
        /// </summary>
        public const string SecuritySchemeName = "ApiKeyScheme";

        /// <summary>
        /// The OpenAPI security scheme description.
        /// </summary>
        public const string SecuritySchemeDescription = "API Key auth";

        /// <summary>
        /// The OpenAPI security scheme reference identifier.
        /// </summary>
        public const string SecuritySchemeReferenceId = "ApiKey";
    }
}
