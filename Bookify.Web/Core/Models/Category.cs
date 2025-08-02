namespace Bookify.Web.Core.Models
{
    /* 
     * This is a database-level constraint, not a validation.
      - It ensures that a certain field(like Name) must be unique in the database table.
      -Even if the form gets submitted, EF Core will throw an error(DbUpdateException) if the value already exists.
      -This protects your data even if JavaScript is disabled or someone bypasses client-side validation.
     *[Remote]
      -This is a client-side validation that sends an AJAX request to the server before submitting the form.
     */

    [Index(nameof(Name), IsUnique = true)]
    public class Category : BaseModel
    {
       
        [MaxLength(100)]
        public string Name { get; set; } = null!;

    }
}
