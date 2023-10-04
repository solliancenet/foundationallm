using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Interfaces;

public interface IVectorDatabaseServiceQueries
{
    Task<string> VectorSearchAsync(float[] embeddings);
}