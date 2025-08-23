using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Rent2Read.Web.Core.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(length: 500, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowItem", null!, AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.DuplicatedBook)]
        public string Title { get; set; } = null!;

        [Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id,Title", ErrorMessage = Errors.DuplicatedBook)]
        public int AuthorId { get; set; }//Foreign Key
        

        [MaxLength(length: 200, ErrorMessage = Errors.MaxLength)]
        public string Publisher { get; set; } = null!;

        
        
        [Display(Name = "Publishing Date")]
        //[AssertThat("PublishingDate <= Today()", ErrorMessage = Errors.NotAllowFutureDates)]
        //Ensure that the PublishingDate is not in the future (must be less than or equal to today's date)

        public DateTime PublishingDate { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }

        [MaxLength(length: 50, ErrorMessage = Errors.MaxLength)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is available for rental ?")]
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;

       
        public IEnumerable<SelectListItem>? Authors { get; set; }//For DropdownList
        //Each SelectListItem represents one option (Text for display, Value for submission).

        public Author? Author { get; set; }  // Navigation Property

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        //I used IList instead of IEnumerable because I need Index to iterate in Controller
        //These contain the ID numbers of the categories that the user has selected.
        public IEnumerable<SelectListItem>? Categories { get; set; }

    }
}
