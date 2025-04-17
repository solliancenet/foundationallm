using FluentValidation;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

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

            When(a => a.Tools.Any(t => t.Category == AgentToolCategories.KnowledgeSearch), () =>
            {
                RuleFor(a => a.Tools.Where(t => t.Category == AgentToolCategories.KnowledgeSearch))
                    .Custom((tools, context) =>
                    {
                        var toolsWithDataPipeline = tools
                            .Where(t => t.TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.FileUploadDataPipeline, out var result1))
                            .Select(t => t.Name)
                            .ToList();

                        if (toolsWithDataPipeline.Count > 1)
                            context.AddFailure($"At most one tool from the {AgentToolCategories.KnowledgeSearch} category is allowed to have a file upload data pipeline in its configuration.");

                        var toolsWithVectorDatabase = tools
                            .Where(t => t.TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.VectorDatabase, out var result2))
                            .Select(t => t.Name)
                            .ToList();

                        if (toolsWithVectorDatabase.Count > 1)
                            context.AddFailure($"At most one tool from the {AgentToolCategories.KnowledgeSearch} category is allowed to have a vector database in its configuration.");

                        if (toolsWithDataPipeline.Count == 1 && toolsWithVectorDatabase.Count == 1
                            && toolsWithDataPipeline[0] != toolsWithVectorDatabase[0])
                            context.AddFailure($"The file upload data pipeline and the vector database must be configured in the same tool.");

                        if ((toolsWithDataPipeline.Count == 1 && toolsWithVectorDatabase.Count == 0)
                            || (toolsWithDataPipeline.Count == 0 && toolsWithVectorDatabase.Count == 1))
                            context.AddFailure($"Both file upload data pipeline and vector database must be configured.");
                    });
            });
        }
    }
}
