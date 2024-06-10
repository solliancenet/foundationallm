using FoundationaLLM.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.SemanticKernel.Core.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the PostgreSQL vector indexing service.
    /// </summary>
    public record PostgresIndexingServiceSettings
    {
        /// <summary>
        /// The connection string for the PostgreSQL server.
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Embedding vector size.
        /// </summary>
        public required string? VectorSize { get; set; }

        /// <summary>
        /// Database schema of the collection tables. Uses the default schema if not specified.
        /// </summary>
        public string? Schema { get; set; }
    }
}
