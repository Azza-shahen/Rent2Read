using Microsoft.AspNetCore.Authorization;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController(ApplicationDbContext _dbContext, IMapper _mapper) : Controller
    {
        #region Index

        public IActionResult Index()
        {
            var authors = _dbContext.Authors.AsNoTracking().ToList();
            var authorVM = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(authorVM);
        }

        #endregion
        #region Create
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var author = _mapper.Map<Author>(model);
                author.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                _dbContext.Authors.Add(author);
                _dbContext.SaveChanges();
                var authorVM = _mapper.Map<AuthorViewModel>(author);

                return PartialView("_AuthorRow", authorVM);
            }
            return BadRequest();
        }

        #endregion

        #region Edit
        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = _dbContext.Authors.Find(id);
            var authorVM = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_Form", authorVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var author = _dbContext.Authors.Find(model.Id);
                if (author is null)
                {
                    return NotFound();
                }
                var name = _mapper.Map(model, author);
                author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                author.LastUpdatedOn = DateTime.Now;
                _dbContext.SaveChanges();

                var authorVM = _mapper.Map<AuthorViewModel>(author);

                return PartialView("_AuthorRow", authorVM);
            }
            return BadRequest();
        }
        #endregion

        #region ToggleStatus
        public IActionResult ToggleStatus(int id)
        {

            var author = _dbContext.Authors.Find(id);
            if (author is null)
            {
                return NotFound();
            }

            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            author.LastUpdatedOn = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok(author.LastUpdatedOn.ToString());


        }
        #endregion

        #region AllowItem
        public IActionResult AllowItem(AuthorFormViewModel model)
        {
            var author = _dbContext.Authors.FirstOrDefault(a => a.Name == model.Name);
            var IsAllowed = author is null || author.Id.Equals(model.Id);

            return Json(IsAllowed);
        }
        #endregion
    }
}
