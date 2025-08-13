using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace Bookify.Web.Controllers
{
    public class BooksController(ApplicationDbContext _dbContext, IMapper _mapper
                                        , IWebHostEnvironment _webHostEnvironment
                                       /* , IOptions<CloudinarySettings> cloudinary*/) : Controller
    { /* IWebHostEnvironment => -Checking environment(IsDevelopment)
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

        #region Details
        public IActionResult Details(int id)
        {
            var book = _dbContext.Books
                .Include(b=>b.Author)
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
                    var extension = Path.GetExtension(model.Image.FileName);

                    if (!_allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                        return View("Form", PopulateViewModel(model));
                    }

                    if (model.Image.Length > _maxAllowedSize)
                    {
                        ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                        return View("Form", PopulateViewModel(model));
                    }

                    var imageName = $"{Guid.NewGuid()}{extension}";//To make sure that the image name will never be repeated
                    var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
                    //put image in wwwroot/images/books.
                    var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books/thumb", imageName);
                    
                    using var stream = System.IO.File.Create(path);
                    //This creates a new empty file in the this path (prepare place for the image to be stored on the server).
                    await model.Image.CopyToAsync(stream);
                    stream.Dispose(); 
                    book.ImageUrl = $"/images/books/{imageName}";
                    book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                    using var image =Image.Load( model.Image.OpenReadStream());
                    var ratio=(float)image.Width / 200;
                    var height = image.Height / ratio;
                    image.Mutate(i=>i.Resize(width:200, height:(int)height));
                    image.Save(thumbPath);

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
               
                var book = _dbContext.Books.Include(b => b.Categories).FirstOrDefault(b => b.Id == model.Id);
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
                    var oldImagePath =$"{_webHostEnvironment.WebRootPath}{book.ImageUrl}";
                        //make sure there is actually file with this image in this location.
                        //So that it doesn't try to delete something that doesn't exist(cause an exception).
                        var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{book.ImageThumbnailUrl}";

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    if (System.IO.File.Exists(oldThumbPath))
                    {
                        System.IO.File.Delete(oldThumbPath);
                    }
                   /* await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);*/
                }


                    var extension = Path.GetExtension(model.Image.FileName);
                     
                    if (!_allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
                        return View("Form", PopulateViewModel(model));
                    }

                    if (model.Image.Length > _maxAllowedSize)
                    {
                        ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
                        return View("Form", PopulateViewModel(model));
                    }
                      
                    var imageName = $"{Guid.NewGuid()}{extension}";//To make sure that the image name will never be repeated

                    var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
                   
                    var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books/thumb", imageName);

                    using var stream = System.IO.File.Create(path);
                    
                    await model.Image.CopyToAsync(stream);
                    stream.Dispose();

                    model.ImageUrl = $"/images/books/{imageName}";
                    model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";
                    using var image = Image.Load(model.Image.OpenReadStream());
                    var ratio = (float)image.Width / 200;
                    var height = image.Height / ratio;
                    image.Mutate(i => i.Resize(width: 200, height: (int)height));
                    image.Save(thumbPath);


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
                _dbContext.SaveChanges();
                return RedirectToAction(nameof(Details), new { id = book.Id });

            }
            return View("Form", PopulateViewModel(model));
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
