namespace Rent2Read.Infrastructure.persistence.Configurations;

internal class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        /* 
    * This is a database-level constraint, not a validation.
     - It ensures that a certain field(like Name) must be unique in the database table.
     -Even if the form gets submitted, EF Core will throw an error(DbUpdateException) if the value already exists.
     -This protects your data even if JavaScript is disabled or someone bypasses client-side validation.
    *[Remote]
     -This is a client-side validation that sends an AJAX request to the server before submitting the form.
    */
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.UserName).IsUnique();

        builder.Property(e => e.FullName).HasMaxLength(100);
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}

