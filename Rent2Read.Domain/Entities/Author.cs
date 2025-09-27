namespace Rent2Read.Domain.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class Author : BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;

    }

}
