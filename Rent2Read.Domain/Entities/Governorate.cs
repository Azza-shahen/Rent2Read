namespace Rent2Read.Domain.Entities
{
    public class Governorate : BaseEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new HashSet<Area>();
    }
}
