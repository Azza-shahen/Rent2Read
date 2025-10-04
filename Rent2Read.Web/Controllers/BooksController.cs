using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rent2Read.Domain.Dtos;
using SixLabors.ImageSharp;



namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController(    IBookService _bookService
                                          , IAuthorService _authorService
                                          , ICategoryService _categoryService
                                          , IMapper _mapper
                                        /* , IWebHostEnvironment _webHostEnvironment*/
                                        , IImageService _imageService
                                        , IValidator<BookFormViewModel> _validator
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

        /*
                //private =>encapsulation
                private readonly List<string> _allowedExtensions = new() { ".png", ".jpg", ".jpeg" };// is used to store the allowed file extensions and size
                private readonly int _maxAllowedSize = 2097152;//2MB*/
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
        [IgnoreAntiforgeryToken]
        public IActionResult GetBooks()
        {
            var form = Request.Form;
            var skip = int.Parse(form["start"]!);
            int pageSize = int.Parse(form["length"]!);
            var searchValue = form["search[value]"];

            var sortColumnIndex = form["order[0][column]"];

            var sortColumn = form[$"columns[{sortColumnIndex}][name]"];
            var sortColumnDirection = form["order[0][dir]"]; // asc or desc

            var filterDto = new FilterationDto(skip, pageSize, searchValue!, sortColumnIndex!, sortColumn!, sortColumnDirection!);

            var (books, recordsTotal) = _bookService.GetFiltered(filterDto);

            var mappedDate = _mapper.Map<IEnumerable<BookViewModel>>(books);

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
        
            var query = _bookService.GetDetails();
            // ProjectTo executes mapping at the SQL level (not in memory)
            var bookVM = _mapper.ProjectTo<BookViewModel>(query)
                         .SingleOrDefault(b => b.Id == id);

            if (bookVM is null)
                return NotFound();

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
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            if (!validationResult.IsValid)
                validationResult.AddToModelState(ModelState);


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
                        ModelState.AddModelError(nameof(Image), result.errorMessage!);
                        return View("Form", PopulateViewModel(model));
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
          

                book = _bookService.Add(book, model.SelectedCategories, User.GetUserId());

                return RedirectToAction(nameof(Details), new { id = book.Id });

            }
            return View("Form", PopulateViewModel(model));
        }
        #endregion
        #region Edit
        [HttpGet]
        public IActionResult Edit(int id)
        { 
            var book = _bookService.GetWithCategories(id);
            if (book == null)
            {
                return NotFound();
            }
            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);
            viewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();

            return View("Form", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            if (!validationResult.IsValid)
                validationResult.AddToModelState(ModelState);

            if (ModelState.IsValid)
            {

                var book = _bookService.GetWithCategories(model.Id);
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
                        _imageService.Delete(book.ImageUrl, book.ImageThumbnailUrl);
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
                else if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    model.ImageUrl = book.ImageUrl;
                    model.ImageThumbnailUrl = book.ImageThumbnailUrl;
                }


                book = _mapper.Map(model, book);


                /* book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl!);*/
                /*  book.ImagePublicId = imagePublicid;*/

            book = _bookService.Update(book, model.SelectedCategories, User.GetUserId());

                return RedirectToAction(nameof(Details), new { id = book.Id });

            }
            return View("Form", PopulateViewModel(model));
        }

        #endregion
        #region ToggleStatus
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = _bookService.ToggleStatus(id, User.GetUserId());

            return book is null ? NotFound() : Ok();
        }

        #endregion
        #region AllowItem
        public IActionResult AllowItem(BookFormViewModel model)
        {
            var IsAllowed = _bookService.AllowTitle(model.Id, model.Title, model.AuthorId);
           
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
            var authors = _authorService.GetActiveAuthors();
            var categories = _categoryService.GetActiveCategories();

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
