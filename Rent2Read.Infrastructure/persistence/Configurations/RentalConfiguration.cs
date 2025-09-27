namespace Rent2Read.Infrastructure.persistence.Configurations
{
    internal class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            // Apply global query filter: exclude Rentals that are marked as deleted
            builder.HasQueryFilter(e => !e.IsDeleted);
            builder.Property(e => e.StartDate).HasDefaultValueSql("CAST(GETDATE() AS Date)");
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }
    }
}
