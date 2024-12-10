using FluentValidation;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.AuthorizationEngine.Validation
{
    /// <summary>
    /// Validator for the <see cref="ActionAuthorizationRequest"/> model.
    /// </summary>
    public class ActionAuthorizationRequestValidator : AbstractValidator<ActionAuthorizationRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="ActionAuthorizationRequest"/> model.
        /// </summary>
        public ActionAuthorizationRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Action)
                .NotNull()
                .NotEmpty()
                .WithMessage("The action must be a valid string.")
                .Must(x => AuthorizableActions.Actions.ContainsKey(x))
                .WithMessage("The action must be a valid action.");

            RuleFor(x => x.UserContext)
                .NotNull()
                .WithMessage("The user context must be a valid object.");

            RuleFor(x => x.UserContext.SecurityPrincipalId)
                .NotNull()
                .NotEmpty()
                .WithMessage("The security principal identifier provided in the user context must be a valid string.")
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("The security principal identifier provided in the user context must be a valid GUID.");

            RuleFor(x => x.UserContext.UserPrincipalName)
                .NotNull()
                .NotEmpty()
                .WithMessage("The user principal name provided in the user context must be a valid string.");

            RuleForEach(x => x.UserContext.SecurityGroupIds)
                .NotNull()
                .NotEmpty()
                .WithMessage("Every security group identifier provided in the user context must be a valid string.")
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("Every security group identifier provided in the user context must be a valid GUID.");

            RuleForEach(x => x.ResourcePaths)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each resource path must be a valid string.");
        }
    }
}
