using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;

namespace Rent2Read.Web.Filters
{
    public class HangfireAuthorizationFilter(string policyName) : IDashboardAuthorizationFilter
    {
        private string _policyName = policyName;
        public bool Authorize([NotNull] DashboardContext context)
        {
            // Get the current HTTP context from Hangfire dashboard request(contains request and user info)
            var httpContext = context.GetHttpContext();

            // Resolve the IAuthorizationService from DI container
            var authService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

            // Execute the authorization policy (_policyName) against the current logged-in user
            // Convert the async result to sync since Hangfire expects a bool return value
            var isAuthorized = authService.AuthorizeAsync(httpContext.User, _policyName)
                                         .ConfigureAwait(false)// avoid deadlocks
                                         .GetAwaiter() // wait for completion
                                         .GetResult() // get the final result
                                         .Succeeded;// true if authorized

            // Return true if the user is authorized to access the dashboard, otherwise false
            return isAuthorized;

        }
    }
}
