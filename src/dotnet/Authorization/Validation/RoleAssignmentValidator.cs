using FluentValidation;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Authorization.Validation
{
    /// <summary>
    /// Validator for the <see cref="RoleAssignment"/> model.
    /// </summary>
    public class RoleAssignmentValidator : AbstractValidator<RoleAssignment>
    {
        /// <summary>
        /// Configured the validation rules for the <see cref="RoleAssignment"/> model.
        /// </summary>
        public RoleAssignmentValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            Include(new ResourceBaseValidator());

            RuleFor(x => x.RoleDefinitionId)
                .NotNull()
                .NotEmpty()
                .WithMessage("The role definition identifier must be a valid string.")
                .Must(x => RoleDefinitions.All.ContainsKey(x))
                .WithMessage("The role definition identifier must be a valid role definition identifier.");

            RuleFor(x => x.PrincipalId)
                .NotNull()
                .NotEmpty()
                .WithMessage("The action must be a valid string.")
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("The principal identifier must be a valid GUID.");

            RuleFor(x => x.PrincipalType)
                .NotNull()
                .NotEmpty()
                .WithMessage("The resource must be a valid string.")
                .Must(x => PrincipalTypes.IsValidPrincipalType(x))
                .WithMessage("The principal type must be a valid principal type.");

            RuleFor(x => x.Scope)
                .NotNull()
                .NotEmpty()
                .WithMessage("The principal identifier must be a valid string.");
        }
    }
}
