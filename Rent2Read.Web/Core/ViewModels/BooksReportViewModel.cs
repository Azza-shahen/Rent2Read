using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rent2Read.Web.Core.ViewModels
{
    public class BooksReportViewModel
    {
        [Display(Name = "Authors")]
        public List<int>? SelectedAuthors { get; set; } = new List<int>();//holds the IDs of the authors chosen by the user.nullable => user can not choose any author

        // Authors is the list of authors that will be displayed in the View
        // as a dropdown or multi-select list (using SelectListItem).
        // Each SelectListItem contains Text (name) and Value (ID).
        public IEnumerable<SelectListItem> Authors { get; set; } = new List<SelectListItem>();

        [Display(Name = "Categories")]
        public List<int>? SelectedCategories { get; set; } = new List<int>(); //holds the IDs of the categories chosen by the user.
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public PaginatedList<Book> Books { get; set; } //Result=> Books contains the paginated list of books after applying filters(Authors + Categories)page by page instead of loading everything at once.
    }

}
