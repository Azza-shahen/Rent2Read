using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController(UserManager<ApplicationUser> _userManager, IMapper _mapper) : Controller
    {
        public async Task<IActionResult >Index()
        {
            var users=await _userManager.Users.ToListAsync();
            var viewModel=_mapper.Map<IEnumerable< UserViewModel>>(users);
            return View(viewModel);
        }
    }
}
