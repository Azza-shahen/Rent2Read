using Microsoft.AspNetCore.Authorization;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController( IAuthorService _authorService
                                    , IMapper _mapper
                                    , IValidator<AuthorFormViewModel> _validator) : Controller
    {
        #region Index

        public IActionResult Index()
        {
            var authors = _authorService.GetAll();
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
        public IActionResult Create(AuthorFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            if (validationResult.IsValid)
            {
                var author = _authorService.Add(model.Name, User.GetUserId());

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
            var author = _authorService.GetById(id);
            var authorVM = _mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_Form", authorVM);
        }

        [HttpPost]

        public IActionResult Edit(AuthorFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            if (validationResult.IsValid)
            {
                var author = _authorService.Update(model.Id, model.Name, User.GetUserId());

                if (author is null)
                {
                    return NotFound();
                }

                var authorVM = _mapper.Map<AuthorViewModel>(author);

                return PartialView("_AuthorRow", authorVM);
            }
            return BadRequest();
        }
        #endregion
        #region ToggleStatus
        public IActionResult ToggleStatus(int id)
        {

            var author = _authorService.ToggleStatus(id, User.GetUserId());
            if (author is null)
            {
                return NotFound();
            }

            return Ok(author.LastUpdatedOn.ToString());


        }
        #endregion
        #region AllowItem
        public IActionResult AllowItem(AuthorFormViewModel model)
        {

            var IsAllowed = _authorService.AllowAuthor(model.Id, model.Name);

            return Json(IsAllowed);
        }
        #endregion
    }
}
