using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using Newtonsoft.Json;

namespace FoundationaLLM.Prompt.Models.Resources
{
    /// <summary>
    /// Provides details about a prompt.
    /// </summary>
    public class PromptReference
    {
        /// <summary>
        /// The name of the prompt.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// The filename of the prompt.
        /// </summary>
        public required string Filename { get; set; }
    }
}
