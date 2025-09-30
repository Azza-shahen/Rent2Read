
using Microsoft.AspNetCore.Authorization;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController(IApplicationDbContext _dbContext
                                           , IMapper _mapper
                                           , IValidator<CategoryFormViewModel> _validator) : Controller
    {

        #region Index
        public IActionResult Index()
        {
            #region Manual Mapping
            /*
               var categories = _dbContext.Categories.Select(c=>new CategoryViewModel
                  {
                 // Projection from the Entity to the ViewModel.don't send all the Entity properties to the View, but only the ones you need.


                Id = c.Id,
                    Name=c.Name,
                    CreatedOn=c.CreatedOn,
                    LastUpdatedOn=c.LastUpdatedOn,
                    IsDeleted=c.IsDeleted
                  })
                   .AsNoTracking().ToList();
    */
            #endregion
            //AsNoTracking=> used with Read-only query(updates are not needed,modifications won’t be saved.)
            var categories = _dbContext.Categories.AsNoTracking().ToList();
            var categoryVM = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(categoryVM);
        }

        #endregion
        #region Create
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }


        [HttpPost]

        /*
         * applies CSRF(Cross-Site Request Forgery) validation to a specific action
         * for every unsafe HTTP request(like POST, PUT, DELETE).
         */

        public IActionResult Create(CategoryFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            if (validationResult.IsValid)//Server Side Validation
            {
                var category = _mapper.Map<Category>(model);
                category.CreatedById = User.GetUserId();

                _dbContext.Categories.Add(category);
                _dbContext.SaveChanges();


                var categoryVM = _mapper.Map<CategoryViewModel>(category);

                return PartialView("_CategoryRow", categoryVM);
            }
            return BadRequest();
        }

        #endregion
        #region Edit
        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category is null)
            {
                return NotFound();
            }
            var CategoryVM = _mapper.Map<CategoryFormViewModel>(category);
            return PartialView("_Form", CategoryVM);
        }

        [HttpPost]

        public IActionResult Edit(CategoryFormViewModel model)
        {
            var validationResult = _validator.Validate(model);
            if (validationResult.IsValid)
            {

                var category = _dbContext.Categories.Find(model.Id);
                if (category is null)
                {
                    return NotFound();
                }
                //category.Name = model.Name;
                category = _mapper.Map(model, category);
                category.LastUpdatedById = User.GetUserId();
                category.LastUpdatedOn = DateTime.Now;
                _dbContext.SaveChanges();


                var categoryVM = _mapper.Map<CategoryViewModel>(category);
                return PartialView("_CategoryRow", categoryVM);

            }
            return BadRequest();
        }

        #endregion
        #region ToggleStatus


        public IActionResult ToggleStatus(int id)
        {

            var category = _dbContext.Categories.Find(id);
            if (category is null)
            {
                return NotFound();
            }

            /*    if (category.IsDeleted)
                {
                    category.IsDeleted = false;
                }
                else
                {
                    category.IsDeleted = true;
                }
            */

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedById = User.GetUserId();
            category.LastUpdatedOn = DateTime.Now;
            _dbContext.SaveChanges();
            return Ok(category.LastUpdatedOn.ToString());
            /*  
             *  We use the value returned by the action (LastUpdatedOn) to update
                this location immediately, without reloading the entire page.
             *  I didn't send IsDeleted too =>because The code knows what it will
              change to, so it doesn't need to know the value from the server.
                  @(Model.IsDeleted ? "Deleted" : "Available")
            */
        }

        #endregion
        #region AllowItem

        //Function to check whether the name that the user entered already exists in the database or not.
        public IActionResult AllowItem(CategoryFormViewModel model)
        {

            var category = _dbContext.Categories.FirstOrDefault(c => c.Name == model.Name);
            var isAllowed = category is null || category.Id.Equals(model.Id);

            return Json(isAllowed);
        }

        #endregion
    }
}
