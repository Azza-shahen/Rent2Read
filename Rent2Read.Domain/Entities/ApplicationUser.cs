
namespace Rent2Read.Domain.Entities
{
    /* [Index(nameof(Email), IsUnique = true)]
     [Index(nameof(UserName), IsUnique = true)]*/
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? CreatedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? LastUpdatedById { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
