namespace Rent2Read.Domain.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Governorate : BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new HashSet<Area>();
    }
}
