namespace Rent2Read.Web.Core.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(NationalId), IsUnique = true)]
    [Index(nameof(MobileNumber), IsUnique = true)]
    public class Subscriber : BaseModel
    {
        [MaxLength(length: 100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(length: 100)]
        public string LastName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        [MaxLength(length: 20)]
        public string NationalId { get; set; } = null!;

        [MaxLength(length: 15)]
        public string MobileNumber { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        [MaxLength(length: 150)]
        public string Email { get; set; } = null!;

        [MaxLength(length: 500)]
        public string ImageUrl { get; set; } = null!;

        [MaxLength(length: 500)]
        public string ImageThumbnailUrl { get; set; } = null!;

        [MaxLength(length: 500)]
        public string Address { get; set; } = null!;
        public bool IsBlackListed { get; set; }

        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; }

        public int AreaId { get; set; }
        public Area? Area { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Rental> Rentals { get; set; } = new List<Rental>();//Subscriber → Rentals is a One-to-Many relationship.

    }
}
