using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Rent2Read.Web.Filters
{
    //Custom Attribute and is made to prevent a specific action from being called unless the request comes from Ajax.
    // I want to prevent people from opening the action directly from the browser(via URL),
    // and instead make this action only run if it comes from a JavaScript Ajax request.
    public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
    {

        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            var request = routeContext.HttpContext.Request;
            var isAjax = request.Headers["X-Requested-With"] == "XMLHttpRequest";
            //It checks if there is a header named X-Requested-With, and its value is XMLHttpRequest,
            //which is what every Ajax request sends automatically.

            return isAjax;
        }


    }
}
