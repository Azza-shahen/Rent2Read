namespace Rent2Read.Infrastructure.persistence.Configurations
{
    internal class RentalCopyConfiguration : IEntityTypeConfiguration<RentalCopy>
    {
        public void Configure(EntityTypeBuilder<RentalCopy> builder)
        {
            builder.HasKey(e => new { e.RentalId, e.BookCopyId });//Composite Key
            builder.HasQueryFilter(e => !e.Rental!.IsDeleted);//exclude RentalCopies that belong to a deleted Rental
            builder.Property(e => e.RentalDate).HasDefaultValueSql("CAST(GETDATE() AS Date)");
        }
    }
}
