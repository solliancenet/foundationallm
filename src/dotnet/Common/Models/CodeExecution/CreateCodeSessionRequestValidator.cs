using FluentValidation;
using FoundationaLLM.Common.Constants.Context;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides validation for the <see cref="CreateCodeSessionRequest"/> model.
    /// </summary>
    public class CreateCodeSessionRequestValidator: AbstractValidator<CreateCodeSessionRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="CreateCodeSessionRequest"/> model.
        /// </summary>
        public CreateCodeSessionRequestValidator()
        {
            RuleFor(request => request.AgentName)
                .NotEmpty()
                .WithMessage("The agent name must be provided.");
            RuleFor(request => request.ConversationId)
                .NotEmpty()
                .WithMessage("The conversation identifier must be provided.");
            RuleFor(request => request.Context)
                .NotEmpty()
                .WithMessage("The context must be provided.");
            RuleFor(request => request.EndpointProvider)
                .NotEmpty()
                .WithMessage("The endpoint provider must be provided.");
            RuleFor(request => request.Language)
                .NotEmpty()
                .WithMessage("The language must be provided.")
                .Must(lang => CodeSessionLanguages.All.Contains(lang))
                .WithMessage($"The language must be one of the following: {string.Join(", ", CodeSessionLanguages.All)}.");
        }
    }
}
