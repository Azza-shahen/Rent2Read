namespace Rent2Read.Web.Core.Models
{
    public class BookCopy : BaseModel
    {

        public int BookId { get; set; }
        public Book? Book { get; set; }
        public bool IsAvailableForRental { get; set; }
        public int EditionNumber { get; set; }
        public int SerialNumber { get; set; }
    }
}
