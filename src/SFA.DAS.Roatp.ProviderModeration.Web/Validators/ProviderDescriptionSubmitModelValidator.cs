using FluentValidation;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.Validators
{
    public class ProviderDescriptionSubmitModelValidator : AbstractValidator<ProviderDescriptionSubmitModel>
    {
        private const string ValidCharactersExpression = @"^[^@#$^=+\\\/<>]*$";
        public const string ProviderDescriptionHasInvalidCharacter = "Your answer must not include any special characters: @, #, $, ^, =, +, \\, /, <, >,";
        public const string ProviderDescriptionEmptyMessage = "Enter provider description";
        public const string ProviderDescriptionLengthErrorMessage = "Provider description must be 750 characters or less";
        public const int ProviderDescriptionMaximumLength = 750;
        public ProviderDescriptionSubmitModelValidator()
        {
            RuleFor(x => x.ProviderDescription)
                .NotEmpty().WithMessage(ProviderDescriptionEmptyMessage)
                .Must(description => !string.IsNullOrEmpty(description) && description.Replace("\r", "").Replace("\n", "").Length <= ProviderDescriptionMaximumLength).WithMessage(ProviderDescriptionLengthErrorMessage)
                .Matches(ValidCharactersExpression).WithMessage(ProviderDescriptionHasInvalidCharacter);
        }
    }
}
