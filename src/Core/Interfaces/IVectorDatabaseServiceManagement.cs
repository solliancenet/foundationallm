using Microsoft.Extensions.Logging;

namespace Solliance.AICopilot.Core.Interfaces;

public interface IVectorDatabaseServiceManagement
{
    Task InsertVector(object document);
    Task InsertVectors(IEnumerable<object> documents);
    Task DeleteVector(object document);
}