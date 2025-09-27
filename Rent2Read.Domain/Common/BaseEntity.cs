

namespace Rent2Read.Domain.Common

{
    public class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string? LastUpdatedById { get; set; }
        public ApplicationUser? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}
