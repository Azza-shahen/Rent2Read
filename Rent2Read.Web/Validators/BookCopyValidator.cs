namespace Rent2Read.Web.Validators
{
    public class BookCopyValidator:AbstractValidator<BookCopyFormViewModel>
    {
        public BookCopyValidator()
        {
            RuleFor(e=>e.EditionNumber).InclusiveBetween(1, 1000).WithMessage(Errors.InvalidRange);
        }
    }
}
