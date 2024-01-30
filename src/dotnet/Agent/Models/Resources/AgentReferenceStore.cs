using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Agent.Models.Resources
{
    /// <summary>
    /// Models the content of the agent reference store managed by the FoundationaLLM.Agent resource provider.
    /// </summary>
    public class AgentReferenceStore
    {
        /// <summary>
        /// The list of all agents registered in the system.
        /// </summary>
        public required List<AgentReference> AgentReferences { get; set; }

        /// <summary>
        /// Creates a string-based dictionary of <see cref="AgentReference"/> values from the current object.
        /// </summary>
        /// <returns>The string-based dictionary of <see cref="AgentReference"/> values from the current object.</returns>
        public Dictionary<string, AgentReference> ToDictionary() =>
            AgentReferences.ToDictionary<AgentReference, string>(ar => ar.Name);

        /// <summary>
        /// Creates a new instance of the <see cref="AgentReferenceStore"/> from a dictionary.
        /// </summary>
        /// <param name="dictionary">A string-based dictionary of <see cref="AgentReference"/> values.</param>
        /// <returns>The <see cref="AgentReferenceStore"/> object created from the dictionary.</returns>
        public static AgentReferenceStore FromDictionary(Dictionary<string, AgentReference> dictionary) =>
            new AgentReferenceStore
            {
                AgentReferences = dictionary.Values.ToList()
            };
    }
}
