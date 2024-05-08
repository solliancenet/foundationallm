using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    /// <summary>
    /// Contains the prompt definitions for use in the FoundationaLLM Core examples.
    /// These definitions are used to create the prompts in the FoundationaLLM Core examples.
    /// </summary>
    public static class PromptCatalog
    {
        #region multipart prompts
        /// <summary>
        /// Catalog of multipart prompts.
        /// </summary>
        public static readonly List<MultipartPrompt> MultipartPrompts =
        [
            new MultipartPrompt
            {
                Name = TestAgentNames.GenericInlineContextAgentName,
                Description = $"Prompt template for the {TestAgentNames.GenericInlineContextAgentName} agent.",
                Prefix = @"You are an analytic agent named Omar that helps people understand the history of the Rosetta Stone.
                    Provide concise answers that are polite and professional.

                    Context:
                    The Rosetta Stone, discovered in 1799 by French soldiers in Egypt, is an ancient stele inscribed with the same text in three scripts: Egyptian hieroglyphs, Demotic script, and Ancient Greek. The stone was found in a small village in the Delta called Rosetta (Rashid). It dates back to 196 BC, during the reign of Pharaoh Ptolemy V. The Rosetta Stone proved crucial in deciphering Egyptian hieroglyphs, primarily through the efforts of the French scholar Jean-François Champollion in 1822. This breakthrough provided the key to understanding much about ancient Egyptian history and culture that had been lost for centuries.
                    The Rosetta Stone is a fragment of a larger stele that originally had no decorative elements but featured a decree affirming the royal cult of the 13-year-old Ptolemy V. The text of the decree was composed by a council of priests to honor the pharaoh. The reasons for the decree and its broader implications on Egyptian society during Ptolemy V’s reign are areas of ongoing research and debate.
                    Today, the Rosetta Stone is housed in the British Museum in London, where it remains one of the most visited and studied artifacts in their collection. Its historical and linguistic significance continues to make it a subject of scholarly and public fascination.
                    "
            },
            new MultipartPrompt
            {
                Name = TestAgentNames.AzureOpenAIDirectInlineContextAgentName,
                Description = $"Prompt template for the {TestAgentNames.AzureOpenAIDirectInlineContextAgentName} agent.",
                Prefix = @"You are an analytic agent named Khalil that helps people find information about FoundationaLLM.
                           Provide concise answers that are polite and professional.

                           Context:
                           FoundationaLLM simplifies and streamlines building knowledge management (e.g., question/answer agents) and analytic (e.g., self-service business intelligence) copilots over the data sources present across your enterprise.
                           FoundationaLLM deploys a secure, comprehensive and highly configurable copilot platform to your Azure cloud environment:
                           - Simplifies integration with enterprise data sources used by agent for in-context learning (e.g., enabling RAG, CoT, ReAct and inner monologue patterns).
                           - Provides defense in depth with fine-grain security controls over data used by agent and pre/post completion filters that guard against attack.
                           - Hardened solution attacked by an LLM red team from inception.
                           - Scalable solution load balances across multiple LLM endpoints.
                           - Extensible to new data sources, new LLM orchestrators and LLMs.
                           You can learn more about FoundationaLLM at https://foundationallm.ai"
            }
        ];
        #endregion

        /// <summary>
        /// Retrieves all prompts defined in the catalog.
        /// </summary>
        /// <returns></returns>
        public static List<PromptBase> GetAllPrompts()
        {
            var prompts = new List<PromptBase>();
            prompts.AddRange(MultipartPrompts);
            
            return prompts;
        }
    }
}
