using FoundationaLLM.Common.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace FoundationaLLM.SemanticKernel.Core.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the Azure Cosmos DB NoSQL vector indexing service.
    /// </summary>
    public record AzureCosmosDBNoSQLIndexingServiceSettings
    {
        /// <summary>
        /// The connection string for the Azure Cosmos DB workspace.
        /// Please note, even though the default connection to the deployed Cosmos DB workspace
        /// uses RBAC, the connection string is required to use the vectorization capabilities
        /// due to the access level required to create workspace artifacts like containers.
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// The name of the Azure Cosmos DB vector database.
        /// Please note, this database should be different from the default one deployed with
        /// the FoundationaLLM platform, as it is used for vectorization purposes.
        /// </summary>
        public string? VectorDatabase { get; set; }

        /// <summary>
        /// Sets the maximum authoscale throughput for new containers automatically created
        /// for this vector database. The default value is 4000 RU/s.
        /// </summary>
        public string AutoscaleMaxThroughput { get; set; } = "4000";

        /// <summary>
        /// Defines the default indexing policy for the vector database.
        /// </summary>
        public VectorEmbeddingPolicy VectorEmbeddingPolicy => new (
        new Collection<Embedding>(
            [
                new Embedding()
            {
                Path = "/embedding",
                DataType = VectorDataType.Float32,
                DistanceFunction = DistanceFunction.Cosine,
                Dimensions = 3072
            }
            ]));

        /// <summary>
        /// Defines the default indexing policy for the vector database.
        /// </summary>
        public IndexingPolicy IndexingPolicy => new()
        {
            VectorIndexes =
            [
                new VectorIndexPath()
                {
                    Path = "/embedding",
                    Type = VectorIndexType.QuantizedFlat
                }
            ]
        };
    }
}
