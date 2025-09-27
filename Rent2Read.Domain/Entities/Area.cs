namespace Rent2Read.Domain.Entities
{
    [Index(nameof(Name), nameof(GovernorateId), IsUnique = true)]
    public class Area : BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; } //Navigation Property

    }
}
