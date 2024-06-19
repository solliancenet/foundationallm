using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using Microsoft.Extensions.Configuration;

namespace FoundationaLLM.Agent.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="KnowledgeManagementAgent"/> model.
    /// </summary>
    public class KnowledgeManagementAgentValidator : AbstractValidator<KnowledgeManagementAgent>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="KnowledgeManagementAgent"/> model.
        /// </summary>
        public KnowledgeManagementAgentValidator()
        {
            Include(new AgentBaseValidator());

            When(x => x.InlineContext == false, () =>
            {
                RuleFor(x => x.Vectorization.IndexingProfileObjectIds).NotEmpty()
                    .WithMessage("Indexing profiles are required for Knowledge Management Agents.");
                RuleFor(x => x.Vectorization.IndexingProfileObjectIds[0]).NotEmpty()
                    .WithMessage("One indexing profile is required.");
                RuleFor(x => x.Vectorization.TextEmbeddingProfileObjectId).NotEmpty()
                    .WithMessage("Embedding profile is required for Knowledge Management Agents.");

                When(x => x.Vectorization.DedicatedPipeline, () =>
                {
                    RuleFor(x => x.Vectorization.DataSourceObjectId).NotEmpty().WithMessage("Data source is required for Knowledge Management Agents with a dedicated pipeline.");
                    RuleFor(x => x.Vectorization.VectorizationDataPipelineObjectId).NotEmpty().WithMessage("Vectorization data pipeline is required for Knowledge Management Agents with a dedicated pipeline.");
                });
            });
        }
    }
}
