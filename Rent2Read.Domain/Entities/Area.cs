namespace Rent2Read.Domain.Entities
{
    public class Area : BaseEntity
    {
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; }
        public Governorate? Governorate { get; set; } //Navigation Property

    }
}
