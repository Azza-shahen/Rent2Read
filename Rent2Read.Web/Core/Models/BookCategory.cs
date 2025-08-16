namespace Rent2Read.Web.Core.Models
{
    public class BookCategory
    {
        //many-to-many Relationship between books and categories
        //Foreign Key(Composite Key)=>this means that every combination of Book and Category will be unique and will not be repeated.
        public int BookId { get; set; }
        public int CategoryId { get; set; }

        // Navigation Properties
        public Book? Book { get; set; }
        public Category? Category { get; set; }
    }
}
