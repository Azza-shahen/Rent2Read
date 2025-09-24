using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenHtmlToPdf;
using Rent2Read.Web.Extensions;
using System.Net.Mime;
using ViewToHTML.Services;


namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class ReportsController(ApplicationDbContext _dbContext
                                             , IMapper _mapper
                                             , IViewRendererService _viewRendererService
                                             , IWebHostEnvironment _webHostEnvironment) : Controller
    {
        private readonly string _logoPath = $"{_webHostEnvironment.WebRootPath}/assets/images/Logo_sm.png";
        private readonly int _sheetStartRow = 5;
        #region Index
        public IActionResult Index()
        {
            return View();
        }
        #endregion
        #region Books
        #region Books

        public IActionResult Books(IList<int> selectedAuthors, IList<int> selectedCategories, int? pageNumber)//selectedAuthors: list of selected author IDs (filters)
        {
            //Load all authors from the database used to build the Authors dropdown filter in the View.
            var authors = _dbContext.Authors
                                    .OrderBy(a => a.Name)
                                    .ToList();

            var categories = _dbContext.Categories
                                       .OrderBy(a => a.Name)
                                       .ToList();

            IQueryable<Book> books = _dbContext.Books
                                               .Include(b => b.Author)
                                               .Include(b => b.Categories)
                                               .ThenInclude(c => c.Category)//(many - to - many relationship).
                                                                            // Apply filtering:
                                                                            //If selectedAuthors list is empty → ignore filter.
                                                                            //     Otherwise → keep only books whose AuthorId exists in selectedAuthors.
                                               .Where(b =>
                                                    (!selectedAuthors.Any() || selectedAuthors.Contains(b.AuthorId)) &&
                                                    (!selectedCategories.Any() || b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)))
                                                );


            // The above ".Where(...)" can be written as "if"
            // if (selectedAuthors.Any())
            //     books = books.Where(b => selectedAuthors.Contains(b.AuthorId));// Author → Book is one-to-many relationship.

            // if (selectedCategories.Any())
            //     books = books.Where(b => b.Categories.Any(c => selectedCategories.Contains(c.CategoryId)));//Book → Categories is many-to-many relationship.


            var viewModel = new BooksReportViewModel
            {
                Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors),
                Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories)
            };

            if (pageNumber is not null)
                viewModel.Books = PaginatedList<Book>.Create(
                                      books,
                                      pageNumber ?? 0,
                                      (int)ReportsConfigurations.PageSize//number of items per page
                );


            return View(viewModel);
        }



        #endregion
        #region ExportBooksToExcel

        public async Task<IActionResult> ExportBooksToExcel(string authors, string categories)
        {
            // Split authors and categories from the query string into arrays so we can filter books by selected IDs.
            var selectedAuthors = authors?.Split(',');
            var selectedCategories = categories?.Split(',');

            var books = _dbContext.Books
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b =>
                            (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString())) // filter by authors
                            && (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString()))) // filter by categories
                        )
                        .ToList();


            using var workbook = new XLWorkbook();//Create a new Excel workbook using ClosedXML Library

            var sheet = workbook.AddWorksheet("Books");// Add a new worksheet named "Books"

            sheet.AddLocalImage(_logoPath);//Insert the logo into the worksheet

            var headerCells = new string[]// Define the table headers
            {
                "Title", "Author", "Categories", "Publisher",
                "Publishing Date", "Hall", "Available for rental", "Status"
            };

            sheet.AddHeader(headerCells);//Add headers to the worksheet

            // Fill the worksheet with book data
            for (int i = 0; i < books.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(books[i].Title);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(books[i].Author!.Name);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(string.Join(", ", books[i].Categories!.Select(c => c.Category!.Name)));
                sheet.Cell(i + _sheetStartRow, 4).SetValue(books[i].Publisher);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(books[i].PublishingDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 6).SetValue(books[i].Hall);
                sheet.Cell(i + _sheetStartRow, 7).SetValue(books[i].IsAvailableForRental ? "Available" : "Not Available");
                sheet.Cell(i + _sheetStartRow, 8).SetValue(books[i].IsDeleted ? "Deleted" : "Available");
            }

            sheet.Format();// Format the worksheet
            sheet.AddTable(books.Count, headerCells.Length);
            sheet.ShowGridLines = false;//Hide default gridlines in the sheet
            await using var stream = new MemoryStream();//Create a memory stream to save the file in memory


            workbook.SaveAs(stream);// Save the workbook to the stream

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Books.xlsx");//Return the file as a downloadable Excel file named "Books.xlsx"
        }

        #endregion
        #region ExportBooksToPDF
        public async Task<IActionResult> ExportBooksToPDF(string authors, string categories)
        {
            // Split the incoming comma-separated authors and categories into string arrays.
            var selectedAuthors = authors?.Split(',');
            var selectedCategories = categories?.Split(',');


            var books = _dbContext.Books
                        .Include(b => b.Author)
                        .Include(b => b.Categories)
                        .ThenInclude(c => c.Category)
                        .Where(b =>
                            (string.IsNullOrEmpty(authors) || selectedAuthors!.Contains(b.AuthorId.ToString())) //If authors empty, include all books.Otherwise,filter by AuthorId (One condition is enough to be true for the whole sentence to return true)                            
                            &&
                            (string.IsNullOrEmpty(categories) || b.Categories.Any(c => selectedCategories!.Contains(c.CategoryId.ToString())))//include books that have at least one CategoryId in the selected list.
                        )
                        .ToList();

            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books);
            var templatePath = "~/Views/Reports/BooksTemplate.cshtml";//Define the Razor view template path that will be used to generate the HTML.

            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, viewModel);// Render the Razor view into an HTML string using service

            // Generate the PDF document from the HTML content.
            var pdf = Pdf
                .From(html)                         // Set the HTML content as the source for the PDF
                .EncodedWith("Utf-8")      // Ensure proper encoding to support special characters(Arabic)
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();  // Convert the configuration into the final PDF content (bytes)

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Books.pdf");
        }


        #endregion
        #endregion


        #region Rentals

        #region Rentals
        public IActionResult Rentals(string duration, int? pageNumber)
        {
            var viewModel = new RentalsReportViewModel { Duration = duration };


            if (!string.IsNullOrEmpty(duration))
            {
                // Try to parse the "from" date (first part before " - ") ex:"2025-01-01 - 2025-01-31"
                if (!DateTime.TryParse(duration.Split(" - ")[0], out DateTime from))
                {
                    ModelState.AddModelError("Duration", Errors.InvalidStartDate); //Add model validation error if parsing fails
                    return View(viewModel);
                }


                if (!DateTime.TryParse(duration.Split(" - ")[1], out DateTime to)) // Try to parse the "to" date (second part after " - ")
                {
                    ModelState.AddModelError("Duration", Errors.InvalidEndDate);
                    return View(viewModel);
                }

                // Using IQueryable allows deferred execution (query is executed later when needed).
                IQueryable<RentalCopy> rentals = _dbContext.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(navigationPropertyPath: r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => r.RentalDate >= from && r.RentalDate <= to);


                if (pageNumber is not null)
                    viewModel.Rentals = PaginatedList<RentalCopy>.Create(
                        rentals,
                        pageNumber ?? 0,
                        (int)ReportsConfigurations.PageSize
                    );
            }

            // Clear ModelState to remove validation errors after processing so they don’t affect further processing of the ViewModel.
            ModelState.Clear();


            return View(viewModel);

        }




        #endregion
        #region ExportRentalsToExcel

        public async Task<IActionResult> ExportRentalsToExcel(string duration)
        {
            var from = DateTime.Parse(duration.Split(" - ")[0]);
            var to = DateTime.Parse(duration.Split(" - ")[1]);

            var rentals = _dbContext.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => !string.IsNullOrEmpty(duration) && r.RentalDate >= from && r.RentalDate <= to)
                        .ToList();

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Rentals");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title",
                "Book Author", "SerialNumber", "Rental Date", "End Date", "Return Date", "Extended On" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < rentals.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(rentals[i].Rental!.Subscriber!.Id);
                sheet.Cell(i + _sheetStartRow, 2).SetValue($"{rentals[i].Rental!.Subscriber!.FirstName} {rentals[i].Rental!.Subscriber!.LastName}");
                sheet.Cell(i + _sheetStartRow, 3).SetValue(rentals[i].Rental!.Subscriber!.MobileNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(rentals[i].BookCopy!.Book!.Title);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(rentals[i].BookCopy!.Book!.Author!.Name);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(rentals[i].BookCopy!.SerialNumber);
                sheet.Cell(i + _sheetStartRow, 7).SetValue(rentals[i].RentalDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(rentals[i].EndDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 9).SetValue(rentals[i].ReturnDate is null ? "-" : rentals[i].ReturnDate?.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 10).SetValue(rentals[i].ExtendedOn is null ? "-" : rentals[i].ExtendedOn?.ToString("d MMM, yyyy"));
            }
            sheet.Format();
            sheet.AddTable(rentals.Count, headerCells.Length);
            sheet.ShowGridLines = false;

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "Rentals.xlsx");
        }
        #endregion
        #region ExportRentalsToPDF
        public async Task<IActionResult> ExportRentalsToPDF(string duration)
        {
            var from = DateTime.Parse(duration.Split(" - ")[0]);
            var to = DateTime.Parse(duration.Split(" - ")[1]);

            var rentals = _dbContext.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .ThenInclude(b => b!.Author)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(r => !string.IsNullOrEmpty(duration) && r.RentalDate >= from && r.RentalDate <= to)
                        .ToList();

            var templatePath = "~/Views/Reports/RentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, rentals);

            var pdf = Pdf
                .From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "Rentals.pdf");
        }

        #endregion
        #endregion
        #region DelayedRentals

        #region DelayedRentals
        public IActionResult DelayedRentals()
        {
            var rentals = _dbContext.RentalCopies
                        .Include(c => c.BookCopy)
                        .ThenInclude(r => r!.Book)
                        .Include(c => c.Rental)
                        .ThenInclude(c => c!.Subscriber)
                        .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today)
                        .ToList();

            var viewModel = _mapper.Map<IEnumerable<RentalCopyViewModel>>(rentals);

            return View(viewModel);
        }
        #endregion
        #region ExportDelayedRentalsToExcel

        public async Task<IActionResult> ExportDelayedRentalsToExcel()
        {
            var rentals = _dbContext.RentalCopies
                           .Include(c => c.BookCopy)
                           .ThenInclude(r => r!.Book)
                           .Include(c => c.Rental)
                           .ThenInclude(c => c!.Subscriber)
                           .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today)
                           .ToList();

            var data = _mapper.Map<IList<RentalCopyViewModel>>(rentals);

            using var workbook = new XLWorkbook();

            var sheet = workbook.AddWorksheet("Delayed Rentals");

            sheet.AddLocalImage(_logoPath);

            var headerCells = new string[] { "Subscriber ID", "Subscriber Name", "Subscriber Phone", "Book Title",
                "Book Serial", "Rental Date", "End Date", "Extended On", "Delay in Days" };

            sheet.AddHeader(headerCells);

            for (int i = 0; i < data.Count; i++)
            {
                sheet.Cell(i + _sheetStartRow, 1).SetValue(data[i].Rental!.Subscriber!.Id);
                sheet.Cell(i + _sheetStartRow, 2).SetValue(data[i].Rental!.Subscriber!.FullName);
                sheet.Cell(i + _sheetStartRow, 3).SetValue(data[i].Rental!.Subscriber!.MobileNumber);
                sheet.Cell(i + _sheetStartRow, 4).SetValue(data[i].BookCopy!.BookTitle);
                sheet.Cell(i + _sheetStartRow, 5).SetValue(data[i].BookCopy!.SerialNumber);
                sheet.Cell(i + _sheetStartRow, 6).SetValue(data[i].RentalDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 7).SetValue(data[i].EndDate.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 8).SetValue(data[i].ExtendedOn is null ? "-" : rentals[i].ExtendedOn?.ToString("d MMM, yyyy"));
                sheet.Cell(i + _sheetStartRow, 9).SetValue(data[i].DelayInDays);
            }

            sheet.Format();
            sheet.AddTable(data.Count, headerCells.Length);
            sheet.ShowGridLines = false;

            await using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(stream.ToArray(), MediaTypeNames.Application.Octet, "DelayedRentals.xlsx");
        }
        #endregion
        #region ExportDelayedRentalsToPDF
        public async Task<IActionResult> ExportDelayedRentalsToPDF()
        {

            var rentals = _dbContext.RentalCopies
                           .Include(c => c.BookCopy)
                           .ThenInclude(r => r!.Book)
                           .Include(c => c.Rental)
                           .ThenInclude(c => c!.Subscriber)
                           .Where(c => !c.ReturnDate.HasValue && c.EndDate < DateTime.Today)
                           .ToList();

            var data = _mapper.Map<IEnumerable<RentalCopyViewModel>>(rentals);

            var templatePath = "~/Views/Reports/DelayedRentalsTemplate.cshtml";
            var html = await _viewRendererService.RenderViewToStringAsync(ControllerContext, templatePath, data);

            var pdf = Pdf
                .From(html)
                .EncodedWith("Utf-8")
                .OfSize(PaperSize.A4)
                .WithMargins(1.Centimeters())
                .Landscape()
                .Content();

            return File(pdf.ToArray(), MediaTypeNames.Application.Octet, "DelayedRentals.pdf");
        }
        #endregion
        #endregion



    }
}
