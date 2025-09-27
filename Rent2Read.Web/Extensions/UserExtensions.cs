namespace Rent2Read.Web.Extensions
{
    public static class UserExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    }
}
