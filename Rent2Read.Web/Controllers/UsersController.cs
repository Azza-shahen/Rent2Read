using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Rent2Read.Web.Core.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController(UserManager<ApplicationUser> _userManager
                                 ,RoleManager<IdentityRole> _roleManager
                                 ,IEmailSender _emailSender     
                                 ,IEmailBody _emailBody
                                 , IMapper _mapper) : Controller
    {
        #region Index
        public async Task<IActionResult> Index()
        {
      
            var users = await _userManager.Users.ToListAsync();
            var viewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModel);
        }

        #endregion
        #region Create
        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var viewModel= new UserFormViewModel {
                Roles = await _roleManager.Roles
                             .Select(r => new SelectListItem
                             {
                                 Text = r.Name,
                                 Value = r.Name
                             })
                             .ToListAsync()
            };
            return PartialView("_Form",viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
         
            ApplicationUser user = new()
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);
               

                var body = _emailBody.GetEmailBody(
            "https://res.cloudinary.com/rent2read/image/upload/v1756294966/icon-positive-vote-1_rdexez_jbv5oh.svg",
                    $"Hey {user.FullName}, thanks for joining us!",
                    "please confirm your email",
                    $"{HtmlEncoder.Default.Encode(callbackUrl!)}",
                    "Active Account!");


                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    body);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }


            return BadRequest(string.Join(',',result.Errors.Select(e=>e.Description)));
        }

        #endregion
        #region Edit
        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var viewModel = _mapper.Map<UserFormViewModel>(user);

            viewModel.SelectedRoles = await _userManager.GetRolesAsync(user);
            viewModel.Roles = await _roleManager.Roles
                                .Select(r => new SelectListItem
                                {
                                    Text = r.Name,
                                    Value = r.Name
                                })
                                .ToListAsync();
      
            return PartialView("_Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]//Protection against CSRF (Cross-Site Request Forgery) attacks, the token must be sent with the form.
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id!);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);//It retrieves the user's current roles 

                var rolesUpdated = !currentRoles.SequenceEqual(model.SelectedRoles);//It compares them with the roles received from the form 

                if (rolesUpdated) //If there is a difference, it means the roles have changed.
                {
                   // It deletes all old roles and adds the new ones.
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                await _userManager.UpdateSecurityStampAsync(user);//forces the user to change the Security Stamp.

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        #endregion
        #region ResetPassword


        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

           var viewModel = new ResetPasswordFormViewModel { Id = user.Id };

            return PartialView("_ResetPasswordForm",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var currentPasswordHash = user.PasswordHash;
            //Save the old PasswordHash, so that if an error occurs, you can reset the old password.

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, model.Password);
            //await _userManager.ResetPasswordAsync(user, token, model.Password);
            //we can use it instead of Remove ,Add
            if (result.Succeeded)
            {
                user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOn = DateTime.Now;

                await _userManager.UpdateAsync(user);

                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UserRow", viewModel);
            }
            else
            {
                user.PasswordHash = currentPasswordHash;
                await _userManager.UpdateAsync(user);

                return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
            }
        }

        #endregion
        #region ToggleStatus

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;

            await _userManager.UpdateAsync(user);
            if (user.IsDeleted)
            {
               await  _userManager.UpdateSecurityStampAsync(user);//forces the user to change the Security Stamp.
            }

            return Ok(user.LastUpdatedOn.ToString());
        }


        #endregion
        #region LockOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
                await _userManager.SetLockoutEndDateAsync(user, lockoutEnd:null);

            return Ok();
        }
        #endregion
        #region AllowEmail
        public async Task<IActionResult> AllowEmail(UserFormViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        }
        #endregion
        #region AllowUserName

        public async Task<IActionResult> AllowUserName(UserFormViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var isAllowed = user is null || user.Id.Equals(model.Id);

            return Json(isAllowed);
        } 
        #endregion
    }
}
