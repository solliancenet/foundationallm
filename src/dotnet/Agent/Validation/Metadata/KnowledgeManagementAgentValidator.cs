using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using Microsoft.IdentityModel.Tokens;

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


            When(x => x.Vectorization != null && x.Vectorization.IndexingProfileObjectIds != null &&
            x.Vectorization.IndexingProfileObjectIds.Any(ipoi => !string.IsNullOrWhiteSpace(ipoi)),
            () =>
            {

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
