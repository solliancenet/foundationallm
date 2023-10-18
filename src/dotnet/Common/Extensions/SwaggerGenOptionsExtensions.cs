using FoundationaLLM.Common.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Extends the SwaggerGenOptions with common options used by the FoundationaLLM APIs.
    /// </summary>
    public static class SwaggerGenOptionsExtensions
    {
        /// <summary>
        /// Extension method that configures API Key auth for the APIs that use it.
        /// </summary>
        /// <param name="options">The base options.</param>
        public static void AddAPIKeyAuth(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition(Swagger.SecurityDefinitionName, new OpenApiSecurityScheme
            {
                Name = HttpHeaders.APIKey,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Description = Swagger.SecuritySchemeDescription,
                Scheme = Swagger.SecuritySchemeName
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = Swagger.SecuritySchemeReferenceId
                        },
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        }
    }
}
