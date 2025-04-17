using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Validation.ResourceProvider
{
    /// <summary>
    /// Validator for the <see cref="AzureCosmosDBResource"/> model.
    /// </summary>
    public class AzureCosmosDBResourceValidator : AbstractValidator<AzureCosmosDBResource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="AzureCosmosDBResource"/> model.
        /// </summary>
        public AzureCosmosDBResourceValidator()
        {
            Include(new ResourceBaseValidator());

            RuleFor(dpr => dpr.Id)
                .NotEmpty()
                .WithMessage("The id is required for the Azure CosmosDB resource.");

            RuleFor(dpr => dpr.UPN)
                .NotEmpty()
                .WithMessage("The user principal name is required for the Azure CosmosDB resource.");

            RuleFor(dpr => dpr.InstanceId)
                .Must(ValidationUtils.ValidateGuid)
                .WithMessage("The FoundationaLLM instance identifier must be a valid GUID.");
        }
    }
}
