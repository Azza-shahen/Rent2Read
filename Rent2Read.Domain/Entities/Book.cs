namespace Rent2Read.Domain.Entities
{
    public class Book : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Publisher { get; set; } = null!;
        public DateTime PublishingDate { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        public string? ImagePublicId { get; set; }
        public string Hall { get; set; } = null!;
        public bool IsAvailableForRental { get; set; }
        public string Description { get; set; } = null!;

        public int AuthorId { get; set; }//Foreign Key

        // Navigation Properties
        public Author? Author { get; set; }
        public ICollection<BookCategory> Categories { get; set; } = new HashSet<BookCategory>();
        public ICollection<BookCopy> Copies { get; set; } = new HashSet<BookCopy>();

    }
}
