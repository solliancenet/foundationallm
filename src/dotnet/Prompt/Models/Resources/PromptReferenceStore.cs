namespace FoundationaLLM.Prompt.Models.Resources
{
    /// <summary>
    /// Models the content of the prompt reference store managed by the FoundationaLLM.Prompt resource provider.
    /// </summary>
    public class PromptReferenceStore
    {
        /// <summary>
        /// The list of all prompts registered in the system.
        /// </summary>
        public required List<PromptReference> PromptReferences { get; set; }

        /// <summary>
        /// Creates a string-based dictionary of <see cref="PromptReference"/> values from the current object.
        /// </summary>
        /// <returns>The string-based dictionary of <see cref="PromptReference"/> values from the current object.</returns>
        public Dictionary<string, PromptReference> ToDictionary() =>
            PromptReferences.ToDictionary<PromptReference, string>(ar => ar.Name);

        /// <summary>
        /// Creates a new instance of the <see cref="PromptReferenceStore"/> from a dictionary.
        /// </summary>
        /// <param name="dictionary">A string-based dictionary of <see cref="PromptReference"/> values.</param>
        /// <returns>The <see cref="PromptReferenceStore"/> object created from the dictionary.</returns>
        public static PromptReferenceStore FromDictionary(Dictionary<string, PromptReference> dictionary) =>
            new PromptReferenceStore
            {
                PromptReferences = dictionary.Values.ToList()
            };
    }
}
