using Microsoft.AspNetCore.Authorization;


namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController(IBookCopyService _bookCopyService
                                       , IBookService _bookService
                                       , IRentalService _rentalService
                                       , IMapper _mapper
                                       , IValidator<BookCopyFormViewModel> _validator) : Controller
    {

        #region Create
        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _bookService.GetById(bookId);

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
            var copy = _bookCopyService.Add(model.BookId, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        #endregion
        #region RentalHistory
        public IActionResult RentalHistory(int id)
        {
            var copyHistory = _rentalService.GetAllByCopyId(id);

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }
        #endregion
        #region Edit
        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _bookCopyService.GetDetails(id);
            //var copy = _dbContext.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);

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

            var copy = _bookCopyService.Update(model.Id, model.EditionNumber, model.IsAvailableForRental, User.GetUserId());

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }


        #endregion
        #region ToggleStatus
        [HttpPost]

        public IActionResult ToggleStatus(int id)
        {

            var copy = _bookCopyService.ToggleStatus(id, User.GetUserId());

            // return copy is null ? NotFound() : Ok();
            if (copy is null)
            {
                return NotFound();
            }

            return Ok();

        }

        #endregion

    }
}
