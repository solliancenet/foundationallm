using FluentValidation;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Validation.ResourceProvider
{
    /// <summary>
    /// Validator for the <see cref="ResourceBase"/> model.
    /// </summary>
    public class ResourceBaseValidator : AbstractValidator<ResourceBase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="ResourceBase"/> model.
        /// </summary>
        public ResourceBaseValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Resource name is required.");
            // Create a rule for the Name property to ensure it is lowercase and contains only letters, numbers, hyphens, and underscores.

            RuleFor(x => x.Type).NotEmpty().WithMessage("Resource type is required.");

            When(x => x.Type == $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}"
                   || x.Type == $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleDefinitions}", () => {
                RuleFor(x => x.Name)
                    .Must(ValidationUtils.ValidateGuid).WithMessage("Resource name must be a valid guid.");
            }).Otherwise(() =>
            {
                RuleFor(x => x.Name)
                    .Matches("^[a-zA-Z]([a-zA-Z0-9_-]?)+$").WithMessage("Resource name must start with a letter and contain only letters, numbers, hyphens, or underscores.");
            });
         
            RuleFor(x => x.ObjectId)
                .Must(ValidationUtils.ValidateObjectId)
                .WithMessage("The object identifier is required and it must be a valid FoundationaLLM object identifier.");
        }
    }
}
