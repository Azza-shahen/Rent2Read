using FluentValidation;
using Microsoft.AspNetCore.Authorization;


namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController(IApplicationDbContext _dbContext
                                       , IMapper _mapper
                                       , IValidator<BookCopyFormViewModel> _validator) : Controller
    {

        #region Create
        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _dbContext.Books.Find(bookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowRentalInput = book.IsAvailableForRental
            };

            return PartialView("Form", viewModel);
        }

        [HttpPost]

        public IActionResult Create(BookCopyFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
                if (!validationResult.IsValid)
                return BadRequest();

            var book = _dbContext.Books.Find(model.BookId);

            if (book is null)
                return NotFound();

            BookCopy copy = new()
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental && model.IsAvailableForRental,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value

            };

            book.Copies.Add(copy);
            _dbContext.SaveChanges();


            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        #endregion
        #region RentalHistory
        public IActionResult RentalHistory(int id)
        {
            var copyHistory = _dbContext.RentalCopies
                .Include(c => c.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(c => c.BookCopyId == id)
                .OrderByDescending(c => c.RentalDate)
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }
        #endregion

        #region Edit
        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _dbContext.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("Form", viewModel);
        }

        [HttpPost]

        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _dbContext.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == model.Id);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && model.IsAvailableForRental;
            copy.LastUpdatedById = User.GetUserId();

            copy.LastUpdatedOn = DateTime.Now;

            _dbContext.SaveChanges();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }


        #endregion
        #region ToggleStatus
        [HttpPost]

        public IActionResult ToggleStatus(int id)
        {

            var copy = _dbContext.BookCopies.Find(id);
            if (copy is null)
            {
                return NotFound();
            }


            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedById = User.GetUserId();

            copy.LastUpdatedOn = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok();

        }

        #endregion

    }
}
