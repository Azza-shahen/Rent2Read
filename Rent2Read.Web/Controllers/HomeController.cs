using HashidsNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace Rent2Read.Web.Controllers
{
    //[Authorize]
    public class HomeController(ApplicationDbContext _dbContext
                                    , IMapper _mapper
                                    , IDataProtectionProvider provider
                                    /*, IHashids _hashids*/) : Controller
    {
        private readonly IDataProtector _dataProtector = provider.CreateProtector("MySecureKey");

        public IActionResult Index()
        {
            if(User.Identity!.IsAuthenticated)//If the user is authenticated (logged in)
                return RedirectToAction(nameof(Index), "Dashboard");

            // Get the last 10 books added(10 newest books)
            var lastAddedBooks = _dbContext.Books
                                .Include(b => b.Author)
                                .Where(b => !b.IsDeleted)
                                .OrderByDescending(b => b.Id)
                                .Take(10)
                                .ToList();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks);

            foreach (var book in viewModel)
               // book.Key = _hashids.EncodeHex(book.Id.ToString());
                book.Key = _dataProtector.Protect(book.Id.ToString());

            return View(viewModel);
        }
         
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
