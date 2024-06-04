using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.AIModel.Validation
{
    /// <summary>
    /// Validator for the <see cref="AIModelBase"/> model.
    /// </summary>
    public class AIModelBaseValidator : AbstractValidator<AIModelBase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="AIModel"/> model.
        /// </summary>
        public AIModelBaseValidator() => Include(new ResourceBaseValidator());
    }
}
