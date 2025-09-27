namespace Rent2Read.Infrastructure.persistence.Configurations
{
    internal class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasIndex(e => new { e.Title, e.AuthorId }).IsUnique();
            //Composite Index=>This index is unique,meaning that the same
            //title is not allowed to be repeated with the same author more than once.

            builder.Property(e => e.Title).HasMaxLength(500);
            builder.Property(e => e.Publisher).HasMaxLength(200);
            builder.Property(e => e.Hall).HasMaxLength(50);
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }
    }
}
