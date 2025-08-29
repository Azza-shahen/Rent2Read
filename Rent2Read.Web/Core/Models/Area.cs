namespace Rent2Read.Web.Core.Models
{
    [Index(nameof(Name), nameof(GovernorateId), IsUnique = true)]
    public class Area:BaseModel
    {
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public int GovernorateId { get; set; } 
        public Governorate? Governorate { get; set; } //Navigation Property


    }
}
