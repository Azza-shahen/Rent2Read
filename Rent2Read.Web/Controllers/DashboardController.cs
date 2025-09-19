using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rent2Read.Web.Controllers
{
    [Authorize]
    public class DashboardController(ApplicationDbContext _dbContext, IMapper _mapper) : Controller
    {

        #region Index
        public IActionResult Index()
        {
            // var numberOfCopies = _dbContext.BookCopies.Count(c => !c.IsDeleted);// Count the number of book copies that are not deleted
            var numberOfCopies = _dbContext.Books.Count(c => !c.IsDeleted);

            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10;//round number of copies down to nearest multiple of 10

            var numberOfsubscribers = _dbContext.Subscribers.Count(c => !c.IsDeleted);

            // Get the last 8 books added(8 newest books)
            var lastAddedBooks = _dbContext.Books
                                .Include(b => b.Author)
                                .Where(b => !b.IsDeleted)
                                .OrderByDescending(b => b.Id)
                                .Take(8)
                                .ToList();

            //Get the top rented books
            var topBooks = _dbContext.RentalCopies
                .Include(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(b => b!.Author)
                .GroupBy(c => new//group all rentals by Book information
                {
                    c.BookCopy!.BookId,
                    c.BookCopy!.Book!.Title,
                    c.BookCopy!.Book!.ImageThumbnailUrl,
                    AuthorName = c.BookCopy!.Book!.Author!.Name
                })
                .Select(b => new
                {
                    b.Key.BookId,
                    b.Key.Title,
                    b.Key.ImageThumbnailUrl,
                    b.Key.AuthorName,
                    Count = b.Count()//number of rentals per book
                })
                .OrderByDescending(b => b.Count) // order books by rental count (most rented first)
                .Take(6)
                // map results into BookViewModel
                .Select(b => new BookViewModel
                {
                    Id = b.BookId,
                    Title = b.Title,
                    ImageThumbnailUrl = b.ImageThumbnailUrl,
                    Author = b.AuthorName
                })
                .ToList();

            var viewModel = new DashboardViewModel
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfsubscribers,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(lastAddedBooks),
                TopBooks = topBooks
            };

            return View(viewModel);
        }
        #endregion
        #region GetRentalsPerDay

        [AjaxOnly]
        public IActionResult GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);//If startDate is not provided, set it to 29 days before today
            endDate ??= DateTime.Today;

            var data = _dbContext.RentalCopies
                .Where(c => c.RentalDate >= startDate && c.RentalDate <= endDate)
                .GroupBy(c => new { Date = c.RentalDate })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.Date.ToString("d MMM"),
                    Value = g.Count().ToString()// Count how many rentals on that day
                })
                .ToList();

            // Create a new list to include all days, even if no rentals happened
            List<ChartItemViewModel> figures = new();

            
            for (var day = startDate; day <= endDate; day = day.Value.AddDays(1))
            {
                // Find the data for this day if it exists, otherwise null
                var dayData = data.SingleOrDefault(d => d.Label == day.Value.ToString("d MMM"));

                ChartItemViewModel item = new()
                {
                    Label = day.Value.ToString("d MMM"),// Current day
                    Value = dayData is null ? "0" : dayData.Value // 0 if no rentals, otherwise the count
                };

                figures.Add(item);
            }
            return Ok(figures);
            // return Ok(data);
        }

        #endregion
        #region GetSubscribersPerCity

        [AjaxOnly]
        public IActionResult GetSubscribersPerCity()
        {
            var data = _dbContext.Subscribers
                .Include(s => s.Governorate)
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.GovernorateName,
                    Value = g.Count().ToString()
                })
                .ToList();

            return Ok(data);
        }

        #endregion
    }
}
