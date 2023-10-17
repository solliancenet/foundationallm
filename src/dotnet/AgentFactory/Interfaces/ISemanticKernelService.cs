namespace FoundationaLLM.AgentFactory.Interfaces
{
    /// <summary>
    /// Interface to define the core methods of a Semantic Kernal Service
    /// </summary>
    public interface ISemanticKernelService : ILLMOrchestrationService
    {

        /// <summary>
        /// Adds an item to memory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemName"></param>
        /// <param name="vectorizer"></param>
        /// <returns></returns>
        Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);

        /// <summary>
        /// Removes an item from memory
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task RemoveMemory(object item);
    }
}
