namespace Rent2Read.Web.Validators
{
    /*            FluentValidation is a library for writing clean, reusable, and maintainable validation rules.
 *            It helps separate validation logic from controllers/ models, improves readability, supports complex*/
    public class AuthorValidator : AbstractValidator<AuthorFormViewModel>
    {
        public AuthorValidator()
        {
            RuleFor(e => e.Name)
                .MaximumLength(100).WithMessage(Errors.MaxLength)
                .Matches(RegexPatterns.CharactersOnly_Eng).WithMessage(Errors.OnlyEnglishLetters);
        }
    }
}
