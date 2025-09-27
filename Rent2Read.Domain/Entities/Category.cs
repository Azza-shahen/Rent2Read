namespace Rent2Read.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public ICollection<BookCategory> Books { get; set; } = new HashSet<BookCategory>();
    }
}
