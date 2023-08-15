using Solliance.AICopilot.Core.Models.Chat;
using Solliance.AICopilot.Core.Models.Search;

namespace Solliance.AICopilot.Core.Interfaces
{
    public interface ISemanticKernelOrchestrationService : ILLMOrchestrationService
    {
        Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);

        Task RemoveMemory(object item);
    }
}
