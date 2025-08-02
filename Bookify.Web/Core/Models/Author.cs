namespace Bookify.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Author : BaseModel
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;

    }

}
