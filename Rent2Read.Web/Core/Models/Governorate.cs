namespace Rent2Read.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Governorate : BaseModel
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<Area> Areas { get; set; } = new HashSet<Area>();
    }
}
