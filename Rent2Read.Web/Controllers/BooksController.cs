using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq.Dynamic.Core;


namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController(ApplicationDbContext _dbContext, IMapper _mapper
                                        , IWebHostEnvironment _webHostEnvironment
                                        ,IImageService _imageService
                                       /* , IOptions<CloudinarySettings> cloudinary*/) : Controller
    { /* IWebHostEnvironment => - to know the location of wwwroot and store images there.
                                -Checking environment(IsDevelopment)
                                - Accessing WebRootPath or ContentRootPath
                                -Managing file uploads or static files    */

        /*      private readonly Cloudinary _cloudinary = new Cloudinary(
               new Account(
                   cloudinary.Value.Cloud,
                   cloudinary.Value.ApiKey,
                   cloudinary.Value.ApiSecret
               )
           );*/


        //private =>encapsulation
        private readonly List<string> _allowedExtensions = new() { ".png", ".jpg", ".jpeg" };// is used to store the allowed file extensions and size
        private readonly int _maxAllowedSize = 2097152;//2MB
        #region Index
        public IActionResult Index()
        {
            return View();
        }

        #endregion


        #region GetBooks

        // This action is used to provide server-side data to DataTables.
        // DataTables sends paging parameters (start, length) in the request.
        // The method reads those values, queries the database, applies paging,
        // and then returns the total count and the current page of data in JSON format.
        // This helps in handling large datasets efficiently by loading only the required rows.
        [HttpPost]
        public IActionResult GetBooks(int start, int length)
        {
            //var skip = int.Parse(Request.Form["start"]);
            //int pageSize = int.Parse(Request.Form["length"]);
            // start = the row you start from.
            //length=>The number of classes you take.
            var searchValue = Request.Form["search[value]"];
           
            var sortColumnIndex = Request.Form["order[0][column]"];
           
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"];
            var sortColumnDirection = Request.Form["order[0][dir]"]; // asc or desc

            IQueryable<Book> books = _dbContext.Books
                .Include(b=>b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);

            if (!string.IsNullOrEmpty(searchValue))
            {
                books = books.Where(b => b.Title.Contains(searchValue!) || b.Author!.Name.Contains(searchValue!));
            }
            // Apply dynamic sorting based on column name and sort direction=>You should use a library like System.Linq.Dynamic.Core
            // not OrderBy from LINQ, because it doesn't understand string as an expression.(b=>b.Title)
            books = books.OrderBy($"{sortColumn} {sortColumnDirection}");

            var data = books.Skip(start).Take(length).ToList();// Returns the required part of the data Only.
            var mappedDate=_mapper.Map<IEnumerable< BookViewModel>>(data);
            var recordsTotal = books.Count(); // Total number of books
            var jsonData = new
            {
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = mappedDate
            };

            return Ok(jsonData);
        }

        #endregion
        #region Details
        public IActionResult Details(int id)
        {
            var book = _dbContext.Books
                .Include(b=>b.Author)
                .Include(b=>b.Copies)
                .Include(b=>b.Categories)
                .ThenInclude(c=>c.Category)
                .SingleOrDefault(b=>b.Id ==id);

            if(book is null)
                return NotFound();
            var bookVM=_mapper.Map<BookViewModel>(book);
            return View(bookVM);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Create()
        {

            return View("Form", PopulateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var book = _mapper.Map<Book>(model);


                if (model.Image is not null)
                {
                    var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";//To make sure that the image name will never be repeated
                    var result = await _imageService.UploadAsync(model.Image, imageName, "/images/books", true);
                    //var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/books", true);
                    if (result.isUploaded)
                    {
                        book.ImageUrl = $"/images/books/{imageName}";
                        book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(Image),result.errorMessage!);
                        return View("Form",PopulateViewModel(model));
                    }
                    

                    /*   using var stream = model.Image.OpenReadStream();
                       // Open a read-only Stream of the uploaded file (IFormFile) to use it for operations such as uploading or processing without saving the file to the server
                       var imageparams = new ImageUploadParams
                       {
                           File = new FileDescription(imageName, stream),
                           UseFilename = true
                       };

                       var result = await _cloudinary.UploadAsync(imageparams);
                       book.ImageUrl = result.SecureUrl.ToString();
                       book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl);
                       book.ImagePublicId = result.PublicId;*/
                }
                book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

                foreach (var category in model.SelectedCategories)
                {
                   /* SelectedCategories contains the IDs of all chosen categories.
                    I loop over them to add each one to the book's Categories collection.
                    This ensures the many-to - many relationship is saved properly in the join table.*/

                      book.Categories.Add(new BookCategory { CategoryId = category });
                }
                _dbContext.Add(book);
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Details),new {id=book.Id});

            }
            return View("Form", PopulateViewModel(model));
        }
        #endregion

        #region Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _dbContext.Books.Include(b=>b.Categories).FirstOrDefault(b=>b.Id==id);
            if (book == null)
            {
                return NotFound();
            }
            var model=_mapper.Map< BookFormViewModel >(book);
            var viewModel = PopulateViewModel(model);
            viewModel.SelectedCategories=book.Categories.Select(c=>c.CategoryId).ToList();
            return View("Form",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (ModelState.IsValid)
            {
               
                var book = _dbContext.Books
                    .Include(navigationPropertyPath: b => b.Categories)
                    .Include(b => b.Copies)
                    .FirstOrDefault(b => b.Id == model.Id);
                if (book == null)
                {
                    return NotFound();
                }
                /* string? imagePublicid = null;*/

                //To delete the old image that was there before the user uploaded a new image for the book
                if (model.Image is not null)
                {
                    if (!string.IsNullOrEmpty(book.ImageUrl))
                {  
                   _imageService.Delete(book.ImageUrl,book.ImageThumbnailUrl);
                   /* await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);*/
                }
                    var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";//To make sure that the image name will never be repeated
                    var (isUploaded, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/books", true);
                    if (isUploaded)
                    {
                        model.ImageUrl = $"/images/books/{imageName}";
                        model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                    }
                    else
                    {
                        ModelState.AddModelError(nameof(Image), errorMessage!);
                        return View("Form", PopulateViewModel(model));
                    }


                    /* using var stream = model.Image.OpenReadStream();
                     // Open a read-only Stream of the uploaded file (IFormFile) to use it for operations such as uploading or processing without saving the file to the server
                     var imageparams = new ImageUploadParams
                     {
                         File = new FileDescription(imageName, stream),
                         UseFilename = true
                     };

                     var result = await _cloudinary.UploadAsync(imageparams);
                     model.ImageUrl = result.SecureUrl.ToString();
                     imagePublicid=result.PublicId;*/


                }
                else if(!string.IsNullOrEmpty(book.ImageUrl)) 
                {
                    model.ImageUrl=book.ImageUrl;
                    model.ImageThumbnailUrl=book.ImageThumbnailUrl;
                }

            
                book=_mapper.Map(model,book);
                book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                book.LastUpdatedOn= DateTime.Now;

               /* book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl!);*/
              /*  book.ImagePublicId = imagePublicid;*/

                foreach (var category in model.SelectedCategories)
                { /* SelectedCategories contains the IDs of all chosen categories.
                  I loop over them to add each one to the book's Categories collection.
                  This ensures the many-to-many relationship is saved properly in the join table.
                  */
                    book.Categories.Add(new BookCategory { CategoryId = category });
                }
                if (!book.IsAvailableForRental)
                {
                    foreach (var copy in book.Copies)
                    {
                      copy.IsAvailableForRental = false;
                    }
                }
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Details), new { id = book.Id });

            }
            return View("Form", PopulateViewModel(model));
        }

        #endregion
        #region ToggleStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {

            var book = _dbContext.Books.Find(id);
            if (book is null)
            {
                return NotFound();
            }

       
            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            book.LastUpdatedOn = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok();
        
        }

        #endregion

        #region AllowItem
        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _dbContext.Books.FirstOrDefault(b => b.Title == model.Title && b.AuthorId==model.AuthorId);
            var IsAllowed = book is null || book.Id.Equals(model.Id);

            return Json(IsAllowed);
        }
        #endregion


        /// <summary>
        ///To reuse the code=>used in Create get and post and Edit( DRY (Don't Repeat Yourself)).
        ///  It loads Authors and Categories from the database.
        ///             Used in both:
        ///             -GET Create: to display dropdowns/lists.
        ///             -POST Create(on error): to redisplay the form with the same dropdowns if validation fails.
        /// </summary>


        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;
            var authors = _dbContext.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _dbContext.Categories.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;

        }

        //takes the original Cloudinary image URL and transforms it into a thumbnail URL.
        //The difference between the 2 URLs is that the /c_thumb,w_200,g_face part is what changes the image format in Cloudinary.
        private string GetThumbnailUrl(string url)
        {

            var separator = "image/upload/";
            var urlParts = url.Split(separator);

            var thumbnailUrl = $"{urlParts[0]}{separator}c_thumb,w_200,g_face/{urlParts[1]}";

            return thumbnailUrl;
        }


    }
}
