using Microsoft.Extensions.Logging;

namespace Solliance.AICopilot.Core.Interfaces;

public interface IVectorDatabaseServiceQueries
{
    Task<string> VectorSearchAsync(float[] embeddings);
}