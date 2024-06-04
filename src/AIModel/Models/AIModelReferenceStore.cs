namespace FoundationaLLM.AIModel.Models
{
    /// <summary>
    /// Models the content of the agent reference store managed by the FoundationaLLM.AIModel resource provider.
    /// </summary>
    public class AIModelReferenceStore
    {
        /// <summary>
        /// The list of all agents registered in the system.
        /// </summary>
        public required List<AIModelReference> AIModelReferences { get; set; }

        /// <summary>
        /// Creates a string-based dictionary of <see cref="AIModelReference"/> values from the current object.
        /// </summary>
        /// <returns>The string-based dictionary of <see cref="AIModelReference"/> values from the current object.</returns>
        public Dictionary<string, AIModelReference> ToDictionary() =>
            AIModelReferences.ToDictionary<AIModelReference, string>(ar => ar.Name);

        /// <summary>
        /// Creates a new instance of the <see cref="AIModelReferenceStore"/> from a dictionary.
        /// </summary>
        /// <param name="dictionary">A string-based dictionary of <see cref="AIModelReference"/> values.</param>
        /// <returns>The <see cref="AIModelReferenceStore"/> object created from the dictionary.</returns>
        public static AIModelReferenceStore FromDictionary(Dictionary<string, AIModelReference> dictionary) =>
            new()
            {
                AIModelReferences = [.. dictionary.Values]
            };
    }
}
