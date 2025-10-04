using DocumentFormat.OpenXml.Vml.Office;
using Humanizer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;


namespace Rent2Read.Web.Controllers
{
    //[Authorize]
    public class HomeController(   IBookService _bookService
                                    , IMapper _mapper
                                    , IDataProtectionProvider provider
                                    /*, IHashids _hashids*/) : Controller
    {
        private readonly IDataProtector _dataProtector = provider.CreateProtector("MySecureKey");

        public IActionResult Index()
        {
            if (User.Identity!.IsAuthenticated)//If the user is authenticated (logged in)
                return RedirectToAction(nameof(Index), "Dashboard");

            //Entity → DTO → ViewModel → View
            // Get the last 10 books added(10 newest books)
            var lastAddedBooks = _bookService.GetLastAddedBooks(10);

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks);

            foreach (var book in viewModel)
                // book.Key = _hashids.EncodeHex(book.Id.ToString());
                book.Key = _dataProtector.Protect(book.Id.ToString());
           
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode = 500)
        {
            return View(new ErrorViewModel { ErrorCode = statusCode, ErrorDescription = ReasonPhrases.GetReasonPhrase(statusCode) });
        }

    }
}
