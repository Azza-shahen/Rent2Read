using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Rent2Read.Web.Helpers
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {

        //Custom Claims Factory adds the FullName as a new Claim,
        //which will help use it easily in Views or Controllers instead of relying only on the UserName or Email.
        public ApplicationUserClaimsPrincipalFactory
            (UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));

            return identity;
        }

    }
}
