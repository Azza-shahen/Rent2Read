namespace Rent2Read.Web.Core.Models
{
    public class BaseModel
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? LastUpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

    }
}
