namespace FoundationaLLM.AgentFactory.Interfaces
{
    public interface ISemanticKernelService : ILLMOrchestrationService
    {
        Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);

        Task RemoveMemory(object item);
    }
}
