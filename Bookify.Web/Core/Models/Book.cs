namespace Bookify.Web.Core.Models
{
    [Index(nameof(Title), nameof(AuthorId), IsUnique = true)]
    //Composite Index=>This index is unique,meaning that the same
    //title is not allowed to be repeated with the same author more than once.
    public class Book : BaseModel
    {
        [MaxLength(length: 500)]
        public string Title { get; set; } = null!;

        [MaxLength(length: 200)]
        public string Publisher { get; set; } = null!;
        public DateTime PublishingDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; }

        [MaxLength(length: 50)]
        public string Hall { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;

        public int AuthorId { get; set; }//Foreign Key

        // Navigation Properties
        public Author? Author { get; set; }
        public ICollection<BookCategory> Categories { get; set; } = new HashSet<BookCategory>();

    }
}
