using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;

namespace FoundationaLLM.SemanticKernel.Skills.Core
{
    /// <summary>
    /// Generic Summarizer Skill class.
    /// </summary>
    public class GenericSummarizerSkill
    {
        private readonly ISKFunction _summarizeConversation;

        /// <summary>
        /// Constructor for the Generic Summarizer Skill.
        /// </summary>
        /// <param name="promptTemplate">The prompt template.</param>
        /// <param name="maxTokens">The maximum number of tokens.</param>
        /// <param name="kernel">The Semantic Kernel service.</param>
        public GenericSummarizerSkill(
            string promptTemplate,
            int maxTokens,
            IKernel kernel)
        {
            _summarizeConversation = kernel.CreateSemanticFunction(
                promptTemplate,
                skillName: nameof(GenericSummarizerSkill),
                description: "Given a section of a conversation transcript, summarize the part of the conversation",
                maxTokens: maxTokens,
                temperature: 0.1,
                topP: 0.5);
        }

        /// <summary>
        /// Gets the summary of a conversation transcript.
        /// </summary>
        /// <param name="input">A short or long conversation transcript.</param>
        /// <param name="context">The Semantic Kernel context.</param>
        /// <returns>The updated Semantic Kernel context.</returns>
        [SKFunction()]
        public Task<SKContext> SummarizeConversationAsync(
            [Description("A short or long conversation transcript.")] string input,
            SKContext context)
        {
            return _summarizeConversation.InvokeAsync(input);
        }
    }
}
