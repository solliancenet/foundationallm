﻿using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Context.Validation
{
    /// <summary>
    /// Provides validation methods for knowledge source objects.
    /// </summary>
    public class KnowledgeSourceValidator : AbstractValidator<KnowledgeSource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="KnowledgeSource"/> model.
        /// </summary>
        public KnowledgeSourceValidator()
        {
            Include(new ResourceBaseValidator());

            RuleFor(vd => vd.EmbeddingModel)
                .NotEmpty()
                .WithMessage("The embedding model is required.");

            RuleFor(vd => vd.VectorDatabaseObjectId)
                .NotEmpty()
                .Must(ValidationUtils.ValidateObjectId)
                .WithMessage("The vector database object identifier is required for the knowledge source and it must be a valid FoundationaLLM object identifier.");
        }
    }
}
