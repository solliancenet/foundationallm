using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Validation.Resources
{
    /// <summary>
    /// Validator for the <see cref="VectorizationProfileBase"/> model.
    /// </summary>
    public class VectorizationProfileBaseValidator : AbstractValidator<VectorizationProfileBase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="VectorizationProfileBase"/> model.
        /// </summary>
        public VectorizationProfileBaseValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("The profile Name is required.");

            // Optionally validate ObjectId if there are specific criteria it needs to meet, such as format.
            RuleFor(x => x.ObjectId)
                .NotEmpty().When(x => x.ObjectId != null)
                .WithMessage("The object ID must not be empty if provided.");

            // Validate Settings dictionary if needed.
            When(x => x.Settings != null && x.Settings.Count != 0, () =>
            {
                RuleForEach(x => x.Settings)
                    .Must((profile, kv) => !string.IsNullOrEmpty(kv.Key))
                    .WithMessage("Settings keys must not be empty.");
            });
        }
    }
}
