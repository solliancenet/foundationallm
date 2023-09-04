using FoundationaLLM.Core.Models.Chat;
using FoundationaLLM.Core.Models.Search;

namespace FoundationaLLM.Core.Interfaces
{
    public interface ISemanticKernelOrchestrationService : ILLMOrchestrationService
    {
        Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);

        Task RemoveMemory(object item);
    }
}
