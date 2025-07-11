﻿namespace FoundationaLLM.Common.Models.ResourceProviders.Prompt
{
    /// <summary>
    /// The category for the prompt class.
    /// </summary>
    public enum PromptCategory
    {
        /// <summary>
        /// Agent prompt.
        /// </summary>
        Agent,

        /// <summary>
        /// Workflow prompt.
        /// </summary>
        Workflow,

        /// <summary>
        /// Tool prompt.
        /// </summary>
        Tool,

        /// <summary>
        /// Data Pipeline prompt.
        /// </summary>
        DataPipeline
    }
}
