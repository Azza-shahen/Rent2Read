using HashidsNet;
using Microsoft.AspNetCore.DataProtection;

namespace Rent2Read.Web.Controllers
{
    public class SearchController(IApplicationDbContext _dbContext
                                            , IDataProtectionProvider provider
                                            , IMapper _mapper
                                            /*, IHashids _hashids*/) : Controller
    {
        private readonly IDataProtector _dataProtector = provider.CreateProtector("MySecureKey");

        #region Index
        public IActionResult Index()
        {
            return View();
        }
        #endregion
        #region Find
        public IActionResult Find(string query)
        {
            var books = _dbContext.Books
                .Include(b => b.Author)
                .Where(b => !b.IsDeleted && (b.Title.Contains(query) || b.Author!.Name.Contains(query)))
                .Select(b => new { b.Title, Author = b.Author!.Name, Key = _dataProtector.Protect(b.Id.ToString())/* Key = _hashids.EncodeHex(b.Id.ToString())*/ })
                .ToList();

            return Ok(books);
        }
        #endregion

        #region Details
        public IActionResult Details(string bKey)
        {
            var bookId = int.Parse(_dataProtector.Unprotect(bKey));
            /*            var bookId = _hashids.DecodeHex(bKey);

                        if (bookId.Length == 0)
                            return NotFound();*/
            if (bookId == 0)
                return NotFound();

            var book = _dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.Id == bookId && !b.IsDeleted);
            //.SingleOrDefault(b => b.Id == int.Parse(bookId) && !b.IsDeleted);

            if (book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);
            viewModel.Key = bKey;
            return View(viewModel);
        }
        #endregion


    }
}
