using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

// This custom TagHelper is created to automatically add the "active" CSS class to <a> links
// when the current controller matches a specified controller name.
// It helps highlight the current page in navigation menus (e.g., in navbars),
// improving UX by showing the user which section they're currently in
namespace Rent2Read.Web.Helpers
{

    [HtmlTargetElement("a", Attributes = "active-when")]//This TagHelper targets <a> tags that have an attribute "active-when"

    public class ActiveTag : TagHelper
    {

        public string? ActiveWhen { get; set; }  // This property holds the controller name(in <a> tags) to compare with the current route

        [ViewContext]
        [HtmlAttributeNotBound]// Prevents this property from being set through HTML

        public ViewContext? ViewContextData { get; set; }// This gets the current ViewContext (routing) injected automatically


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(ActiveWhen))// If no value was provided for active-when, do nothing
                return;

            // Get the current controller name from the route
            var currentController = ViewContextData?.RouteData.Values["controller"]?.ToString() ?? string.Empty;

            // Compare it with the ActiveWhen value provided by the developer
            if (currentController!.Equals(ActiveWhen))
            {
                // If the <a> already has a class attribute, append 'active' to it
                if (output.Attributes.ContainsName("class"))
                    output.Attributes.SetAttribute("class", $"{output.Attributes["class"].Value} active");
                else
                    // If no class attribute, add a new one with value 'active'
                    output.Attributes.SetAttribute("class", "active");
            }

        }
    }
}
